<#
.SYNOPSIS
    Fills the deployed demo with claims, parties, reserves and documents for testing.

.DESCRIPTION
    Run it yourself in Windows PowerShell after `az login`. It does not touch the database
    or a migration: it signs in as the three demo users and calls the same API endpoints the
    frontend uses, so every claim is valid by construction (domain rules, audit entries,
    stored issues, balances and the claim numbers all come out as the application makes them).

    What it creates (a mix by position in the run, so lists, filters and paging have data):
      - Draft claims (some with an initial reserve)
      - Open claims with reserves approved by a supervisor or a manager, some with a second
        component, an adjustment or a reversal
      - Under investigation, pending payment, closed and withdrawn claims
      - Open claims whose reserve is left waiting for approval, or was rejected
      - A PDF document on some claims (this also exercises the Azure Blob adapter)

    Every step is attempted on its own: a refusal is counted and shown, and the run goes on.
    It stops before doing anything when claims already exist, unless -Force is given.

.PARAMETER Count
    How many claims to create (default 80; the list pages hold 25 by default).

.PARAMETER ResourceGroup
    The resource group of the demo (default rg-claims-demo).

.PARAMETER AccessCode
    The dev sign-in access code. Read from Key Vault when left out.

.PARAMETER Force
    Create the claims even when claims already exist.

.EXAMPLE
    .\infra\seed-demo-data.ps1 -Count 120
#>
[CmdletBinding()]
param(
    [int]$Count = 80,
    [string]$ResourceGroup = 'rg-claims-demo',
    [string]$AccessCode,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$script:Failures = New-Object System.Collections.ArrayList

function Get-AzureValue([string]$What, [string[]]$Arguments) {
    $value = & az @Arguments
    if ($LASTEXITCODE -ne 0 -or -not $value) {
        throw "$What could not be read from Azure (run az login first)."
    }

    return $value
}

function Get-ErrorText($Data, [string]$Text) {
    if ($null -ne $Data -and $null -ne $Data.errors) {
        $messages = foreach ($property in $Data.errors.PSObject.Properties) { $property.Value }

        return ($messages -join '; ')
    }

    return $Text
}

# One place for every call: it never throws, it returns the status, the parsed body and the error text.
function Invoke-Api([string]$Method, [string]$Path, [string]$Token, $Body) {
    $arguments = @{
        Method          = $Method
        Uri             = "$script:ApiUrl$Path"
        UseBasicParsing = $true
        TimeoutSec      = 120
        Headers         = @{}
    }
    if ($Token) {
        $arguments.Headers['Authorization'] = "Bearer $Token"
    }
    if ($null -ne $Body) {
        $arguments['Body'] = ConvertTo-Json -InputObject $Body -Depth 8 -Compress
        $arguments['ContentType'] = 'application/json'
    }

    $text = ''
    $status = 0
    try {
        $response = Invoke-WebRequest @arguments
        $status = [int]$response.StatusCode
        $text = $response.Content
    }
    catch [System.Net.WebException] {
        $failed = $_.Exception.Response
        if ($null -eq $failed) {
            return [pscustomobject]@{ Ok = $false; Status = 0; Data = $null; Error = $_.Exception.Message }
        }
        $status = [int]$failed.StatusCode
        $text = (New-Object System.IO.StreamReader($failed.GetResponseStream())).ReadToEnd()
    }

    $data = $null
    if ($text) {
        try { $data = $text | ConvertFrom-Json } catch { $data = $null }
    }

    return [pscustomobject]@{
        Ok     = ($status -ge 200 -and $status -lt 300)
        Status = $status
        Data   = $data
        Error  = (Get-ErrorText $data $text)
    }
}

function Confirm-Step([string]$Label, $Result) {
    if (-not $Result.Ok) {
        [void]$script:Failures.Add("$Label -> $($Result.Status) $($Result.Error)")
    }

    return $Result.Ok
}

function Get-Token([string]$UserId) {
    $headers = @{ 'X-DevAuth-Secret' = $script:AccessCode }
    $response = Invoke-RestMethod -Method Post -Uri "$script:ApiUrl/api/dev/token" -Headers $headers `
        -ContentType 'application/json' -Body (ConvertTo-Json -Compress @{ userId = $UserId })

    return $response.token
}

function Format-Utc([datetime]$Value) {
    return $Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
}

# --- Lookups -------------------------------------------------------------------------------------------------

$appName = Get-AzureValue 'The web app name' @('webapp', 'list', '--resource-group', $ResourceGroup, '--query', '[0].name', '-o', 'tsv')
$hostName = Get-AzureValue 'The web app host name' @('webapp', 'list', '--resource-group', $ResourceGroup, '--query', '[0].defaultHostName', '-o', 'tsv')
$script:ApiUrl = "https://$hostName"

if (-not $AccessCode) {
    $vault = Get-AzureValue 'The key vault name' @('keyvault', 'list', '--resource-group', $ResourceGroup, '--query', '[0].name', '-o', 'tsv')
    $AccessCode = Get-AzureValue 'The access code' @('keyvault', 'secret', 'show', '--vault-name', $vault, '--name', 'devauth-secret', '--query', 'value', '-o', 'tsv')
}
$script:AccessCode = $AccessCode

# The three demo users come from the web app's own settings, so nothing is repeated here.
$settings = (Get-AzureValue 'The app settings' @('webapp', 'config', 'appsettings', 'list', '--resource-group', $ResourceGroup, '--name', $appName, '-o', 'json') | Out-String) | ConvertFrom-Json
$usersByIndex = @{}
foreach ($setting in $settings) {
    if ($setting.name -match '^DevAuth__Users__(\d+)__(Id|Role)$') {
        if (-not $usersByIndex.ContainsKey($Matches[1])) { $usersByIndex[$Matches[1]] = @{} }
        $usersByIndex[$Matches[1]][$Matches[2]] = $setting.value
    }
}
$userIdByRole = @{}
foreach ($user in $usersByIndex.Values) { $userIdByRole[$user['Role']] = $user['Id'] }
foreach ($role in 'Handler', 'Supervisor', 'Manager') {
    if (-not $userIdByRole.ContainsKey($role)) { throw "The demo user with the role $role is not configured." }
}

Write-Host "API: $script:ApiUrl"
$handler = Get-Token $userIdByRole['Handler']
$supervisor = Get-Token $userIdByRole['Supervisor']
$manager = Get-Token $userIdByRole['Manager']

$existing = Invoke-Api 'GET' '/api/claims?pageSize=1' $handler $null
if (-not $existing.Ok) { throw "The claim list could not be read: $($existing.Status) $($existing.Error)" }
if ($existing.Data.totalCount -gt 0 -and -not $Force) {
    "The environment already holds $($existing.Data.totalCount) claims. Run again with -Force to add $Count more."
    return
}

$policies = (Invoke-Api 'GET' '/api/policies/search?pageSize=100' $handler $null).Data.items
$causes = @((Invoke-Api 'GET' '/api/reference/cause-of-loss-codes' $handler $null).Data | ForEach-Object { $_.code })
if (@($policies).Count -eq 0 -or $causes.Count -eq 0) { throw 'The policies or the cause codes could not be read.' }

# --- Data pools ----------------------------------------------------------------------------------------------

$firstNames = 'Ada', 'Boris', 'Clara', 'Dmitri', 'Elena', 'Felix', 'Greta', 'Hugo', 'Irina', 'Jonas', 'Karina', 'Leon'
$lastNames = 'Stone', 'Novak', 'Keller', 'Moreau', 'Petrov', 'Silva', 'Lindqvist', 'Rossi', 'Haas', 'Costa'
$companies = 'Northwind Logistics', 'Bluebird Foods', 'Harbor Print Works', 'Summit Builders', 'Greenfield Farms'
$locations = 'Warehouse 4', 'Main street 12', 'Kitchen', 'Parking lot B', 'Ring road exit 8', 'Basement archive', 'Loading dock'
$assets = @(
    @{ Type = 'Vehicle'; Text = 'Delivery van' },
    @{ Type = 'Property'; Text = 'Office building' },
    @{ Type = 'Person'; Text = 'Driver' },
    @{ Type = 'Equipment'; Text = 'Industrial oven' },
    @{ Type = 'Other'; Text = 'Stored goods' }
)
$descriptions = @(
    'Water damage after a pipe burst',
    'Collision at a junction in heavy rain',
    'Break-in overnight, stock removed',
    'Fire started in the electrical cabinet',
    'Storm damage to the roof and windows',
    'Equipment stopped working after a power surge',
    'Visitor slipped on a wet floor',
    'Flooding after heavy rain in the basement'
)
$amounts = 1500, 2500, 4800, 7200, 9000, 12500, 18000, 32000, 56000, 88000, 120000, 240000

$today = (Get-Date).ToUniversalTime().Date
$pdfPath = Join-Path $env:TEMP 'demo-claim-document.pdf'
[System.IO.File]::WriteAllText($pdfPath, "%PDF-1.4`n1 0 obj<</Type/Catalog>>endobj`ntrailer<</Root 1 0 R>>`n%%EOF`n")

function New-ClaimBody([int]$Index, $Policy, [bool]$WithReserve) {
    # The loss date lies inside the policy period and in the past, spread over the last year where possible.
    $latest = $today.AddDays(-2)
    $end = [datetime]$Policy.expirationDate
    if ($end -gt $latest) { $end = $latest }
    $from = [datetime]$Policy.effectiveDate
    if ($from -lt $latest.AddDays(-400)) { $from = $latest.AddDays(-400) }
    if ($from -gt $end) { $from = [datetime]$Policy.effectiveDate }
    $span = [int]($end - $from).TotalDays
    $lossDate = $from.AddDays(($Index * 37) % ($span + 1))
    $reportDate = $lossDate.AddDays(2)
    if ($reportDate -gt (Get-Date).ToUniversalTime().AddHours(-1)) { $reportDate = (Get-Date).ToUniversalTime().AddHours(-1) }

    $first = $firstNames[$Index % $firstNames.Count]
    $last = $lastNames[$Index % $lastNames.Count]
    $amount = $amounts[$Index % $amounts.Count]
    $parties = @(@{ Role = 'Claimant'; Type = 'Person'; FirstName = $first; LastName = $last; Email = "$($first.ToLower()).$($last.ToLower())@example.com"; Phone = "+4850$('{0:D7}' -f ($Index * 7919 % 10000000))" })
    if ($Index % 3 -eq 0) { $parties += @{ Role = 'Insured'; Type = 'Company'; CompanyName = $companies[$Index % $companies.Count] } }
    if ($Index % 4 -eq 0) { $parties += @{ Role = 'ThirdParty'; Type = 'Person'; FirstName = $firstNames[($Index + 5) % $firstNames.Count]; LastName = $lastNames[($Index + 3) % $lastNames.Count] } }
    if ($Index % 7 -eq 0) { $parties += @{ Role = 'Witness'; Type = 'Person'; FirstName = 'Alex'; LastName = $lastNames[($Index + 1) % $lastNames.Count] } }
    if ($Index % 11 -eq 0) { $parties += @{ Role = 'Attorney'; Type = 'Company'; CompanyName = 'Lex and Partners' } }

    $asset = $assets[$Index % $assets.Count]
    $reserves = @()
    if ($WithReserve) {
        $reserves = @(@{ TransactionType = 'Add'; Amount = $amount; ChangeReason = 'Initial reserve'; ComponentType = 'Indemnity' })
    }

    return @{
        PolicyId    = $Policy.id
        ReportDate  = (Format-Utc $reportDate)
        Loss        = @{
            LossDate            = (Format-Utc $lossDate)
            Description         = $descriptions[$Index % $descriptions.Count]
            CauseOfLossCode     = $causes[$Index % $causes.Count]
            Location            = $locations[$Index % $locations.Count]
            EstimatedLossAmount = [math]::Round($amount * 1.2)
        }
        Parties     = $parties
        RiskObjects = @(@{ AssetType = $asset.Type; AssetDescription = $asset.Text; IsPrimary = $true; DamageDescription = 'See the loss description' })
        Reserves    = $reserves
    }
}

# Supervisors approve up to 100,000 and managers above; a handler cannot approve their own submission.
function Approve-Pending([string]$ClaimId, [string]$Label) {
    $reserves = Invoke-Api 'GET' "/api/claims/$ClaimId/reserves" $handler $null
    if (-not (Confirm-Step "$Label read reserves" $reserves)) { return }

    foreach ($row in @($reserves.Data.history | Where-Object { $_.approvalStatus -eq 'PendingApproval' })) {
        $approver = if ([math]::Abs([decimal]$row.amount) -gt 100000) { $manager } else { $supervisor }
        [void](Confirm-Step "$Label approve $($row.amount)" (Invoke-Api 'POST' "/api/claims/$ClaimId/reserves/$($row.id)/approve" $approver $null))
    }
}

function Submit-Reserve([string]$ClaimId, [string]$Label, $Body) {
    return Confirm-Step "$Label reserve $($Body.Amount)" (Invoke-Api 'POST' "/api/claims/$ClaimId/reserves" $handler $Body)
}

function Send-Document([string]$ClaimId, [string]$Label) {
    $url = "$script:ApiUrl/api/claims/$ClaimId/documents"
    $code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 120 -X POST $url -H "Authorization: Bearer $handler" `
        -F "file=@$pdfPath;type=application/pdf" -F 'documentType=Police report' -F 'notes=Seeded demo document'
    if ($code -ne '201') { [void]$script:Failures.Add("$Label upload -> $code") }
}

# --- The run -------------------------------------------------------------------------------------------------

$kinds = 'Draft', 'DraftWithReserve', 'Open', 'Open', 'OpenWithExtras', 'UnderInvestigation', 'PendingPayment', 'Closed', 'Withdrawn', 'OpenWaiting'
$created = @{}
$policyList = @($policies)

for ($index = 1; $index -le $Count; $index++) {
    $kind = $kinds[$index % $kinds.Count]
    $policy = $policyList[$index % $policyList.Count]
    $isDraft = $kind -like 'Draft*'
    $hasReserve = $kind -ne 'Draft' -and $kind -ne 'Withdrawn'
    $label = "[$index/$Count $kind]"

    $response = Invoke-Api 'POST' '/api/claims' $handler (New-ClaimBody $index $policy $hasReserve)
    if (-not (Confirm-Step "$label create" $response)) { continue }
    $claimId = $response.Data.data.id
    Write-Host "$label $($response.Data.data.claimNumber)"

    if ($isDraft) { $created[$kind] = 1 + [int]$created[$kind]; continue }

    if ($index % 6 -eq 0) { Send-Document $claimId $label }

    if (-not (Confirm-Step "$label open" (Invoke-Api 'PUT' "/api/claims/$claimId/status" $handler @{ TargetStatus = 'Open' }))) { continue }
    if ($hasReserve) { Approve-Pending $claimId $label }

    if ($kind -eq 'OpenWithExtras') {
        [void](Submit-Reserve $claimId $label @{ TransactionType = 'Add'; Amount = 2000; ChangeReason = 'Expert fees'; ComponentType = 'Expense' })
        $components = Invoke-Api 'GET' "/api/claims/$claimId/reserves" $handler $null
        $indemnity = @($components.Data.components | Where-Object { $_.componentType -eq 'Indemnity' })[0]
        if ($null -ne $indemnity) {
            [void](Submit-Reserve $claimId $label @{ TransactionType = 'Adjust'; Amount = 3000; ChangeReason = 'Updated estimate'; ComponentId = $indemnity.id })
            [void](Submit-Reserve $claimId $label @{ TransactionType = 'Reverse'; Amount = -500; ChangeReason = 'Duplicate part'; ComponentId = $indemnity.id })
        }
        Approve-Pending $claimId $label
    }

    if ($kind -eq 'OpenWaiting') {
        # An approved reserve is already there; this one is left waiting, and a second one is rejected.
        [void](Submit-Reserve $claimId $label @{ TransactionType = 'Add'; Amount = 15000; ChangeReason = 'Waiting for approval'; ComponentType = 'Alae' })
        if (Submit-Reserve $claimId $label @{ TransactionType = 'Add'; Amount = 20000; ChangeReason = 'Too high'; ComponentType = 'SubrogationRecoverable' }) {
            $reserves = Invoke-Api 'GET' "/api/claims/$claimId/reserves" $handler $null
            $row = @($reserves.Data.history | Where-Object { $_.approvalStatus -eq 'PendingApproval' -and $_.amount -eq 20000 })[0]
            if ($null -ne $row) {
                [void](Confirm-Step "$label reject" (Invoke-Api 'POST' "/api/claims/$claimId/reserves/$($row.id)/reject" $supervisor @{ rejectionReason = 'Not supported by the file' }))
            }
        }
    }

    switch ($kind) {
        'UnderInvestigation' { $move = @{ TargetStatus = 'UnderInvestigation'; Note = 'Expert opinion requested' } }
        'PendingPayment' { $move = @{ TargetStatus = 'PendingPayment'; IsLiabilityConfirmed = $true; Note = 'Liability confirmed' } }
        'Closed' { $move = @{ TargetStatus = 'Closed'; ReasonCode = 'SETTLED'; ReasonText = 'Settled with the claimant'; Note = 'Closed after settlement' } }
        'Withdrawn' { $move = @{ TargetStatus = 'Withdrawn'; ReasonCode = 'CLAIMANT_REQUEST'; ReasonText = 'The claimant withdrew the claim'; Note = 'No reserve was set' } }
        default { $move = $null }
    }
    if ($null -ne $move) {
        [void](Confirm-Step "$label move to $($move.TargetStatus)" (Invoke-Api 'PUT' "/api/claims/$claimId/status" $handler $move))
    }

    $created[$kind] = 1 + [int]$created[$kind]
}

Remove-Item $pdfPath -ErrorAction SilentlyContinue

# --- Summary -------------------------------------------------------------------------------------------------

''
'Claims by status now:'
foreach ($status in 'Draft', 'Open', 'UnderInvestigation', 'PendingPayment', 'Closed', 'Reopened', 'Withdrawn') {
    $page = Invoke-Api 'GET' "/api/claims?status=$status&pageSize=1" $handler $null
    "  {0,-20} {1}" -f $status, $page.Data.totalCount
}

''
"Refused or failed steps: $($script:Failures.Count)"
foreach ($failure in ($script:Failures | Select-Object -First 25)) { "  $failure" }
if ($script:Failures.Count -gt 25) { "  ... and $($script:Failures.Count - 25) more" }
