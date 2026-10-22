<#
.SYNOPSIS
    Fills the deployed demo with realistic claims for testing lists, filters, paging and the reserve workflow.

.DESCRIPTION
    Run it yourself in Windows PowerShell after `az login`. It does not touch the database or a migration:
    it signs in as the three demo users and calls the same API endpoints the frontend uses, so every claim is
    valid by construction (domain rules, audit entries, stored issues, balances and claim numbers).

    What it creates (random but repeatable: the same -Seed gives the same data):
      - Claims created in loss-date order, so the claim numbers rise with the loss dates (the list is sorted by
        number). Loss dates spread over the last -WindowDays days, each inside the period of a policy.
      - A normal split of statuses: roughly 16% draft, 22% open, 15% under investigation, 11% pending payment,
        20% closed, 10% withdrawn and 6% reopened, leaning by age (old claims towards closed or withdrawn,
        recent ones towards draft or open).
      - Handlers split between the three demo users, policies, causes of loss, parties, risk objects and loss
        amounts chosen independently.
      - Zero to three reserve components per claim, each with a history of additions, adjustments and
        reversals in mixed states: approved, waiting for approval and rejected.
      - PDF documents and notes on some claims (the documents also exercise the Azure Blob adapter).
      - Reserves at the limits: three claims reach the 10 million claim limit (approvals that would pass it are
        refused) and five sit at the edges of the approval tiers (10,000 approves itself, 100,000 needs a
        supervisor, 100,050 a manager). Rows end approved, auto-approved, rejected, retracted or waiting.
      - Validation issues in every status: some claims have a policy that does not cover the loss date (the warning
        is acknowledged before opening, or left raised on a draft), a too short description or a loss date in the
        future (critical, waived by a supervisor, or left raised on a draft), an unknown cause of loss or no
        claimant (critical, cannot be waived, so the claim stays a draft) and no risk object (a warning that stays
        raised).

    Every step is attempted on its own: a refusal is counted and shown, and the run goes on. It stops before
    doing anything when claims already exist, unless -Force is given.

.PARAMETER Count
    How many claims to create (default 100; the lists page by 25).

.PARAMETER ResourceGroup
    The resource group of the demo (default rg-claims-demo).

.PARAMETER Seed
    The random seed (default 2026). The same seed gives the same claims.

.PARAMETER WindowDays
    How far back the loss dates reach (default 240 days).

.PARAMETER Force
    Create the claims even when claims already exist.

.EXAMPLE
    .\infra\seed-demo-data.ps1 -Count 150
#>
[CmdletBinding()]
param(
    [int]$Count = 100,
    [string]$ResourceGroup = 'rg-claims-demo',
    [int]$Seed = 2026,
    [int]$WindowDays = 240,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$script:Failures = New-Object System.Collections.ArrayList
$script:ExpectedRefusals = New-Object System.Collections.ArrayList
$script:LimitClaims = 0
$script:Random = New-Object System.Random($Seed)

# --- Helpers -------------------------------------------------------------------------------------------------

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
    $response = Invoke-RestMethod -Method Post -Uri "$script:ApiUrl/api/dev/token" `
        -ContentType 'application/json' -Body (ConvertTo-Json -Compress @{ userId = $UserId })

    return $response.token
}

function Format-Utc([datetime]$Value) {
    return $Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
}

function Get-Int([int]$Min, [int]$Max) {
    return $script:Random.Next($Min, $Max + 1)
}

function Get-Chance([double]$Probability) {
    return $script:Random.NextDouble() -lt $Probability
}

function Get-OneOf($List) {
    $items = @($List)

    return $items[$script:Random.Next($items.Count)]
}

# Weights are a name -> number table; the keys are sorted so the same seed always gives the same pick.
function Get-Weighted([hashtable]$Weights) {
    $keys = @($Weights.Keys | Sort-Object)
    $total = 0
    foreach ($key in $keys) { $total += $Weights[$key] }
    $point = $script:Random.NextDouble() * $total
    foreach ($key in $keys) {
        $point -= $Weights[$key]
        if ($point -lt 0) { return $key }
    }

    return $keys[-1]
}

# An amount between the limits where small values are much more common than large ones, rounded to 50.
function Get-Amount([double]$Min, [double]$Max) {
    $logMin = [math]::Log($Min)
    $logMax = [math]::Log($Max)
    $value = [math]::Exp($logMin + $script:Random.NextDouble() * ($logMax - $logMin))

    return [decimal]([math]::Round($value / 50) * 50)
}

# --- Lookups -------------------------------------------------------------------------------------------------

$appName = Get-AzureValue 'The web app name' @('webapp', 'list', '--resource-group', $ResourceGroup, '--query', '[0].name', '-o', 'tsv')
$hostName = Get-AzureValue 'The web app host name' @('webapp', 'list', '--resource-group', $ResourceGroup, '--query', '[0].defaultHostName', '-o', 'tsv')
$script:ApiUrl = "https://$hostName"

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
$tokens = @{
    Handler    = Get-Token $userIdByRole['Handler']
    Supervisor = Get-Token $userIdByRole['Supervisor']
    Manager    = Get-Token $userIdByRole['Manager']
}

$existing = Invoke-Api 'GET' '/api/claims?pageSize=1' $tokens.Handler $null
if (-not $existing.Ok) { throw "The claim list could not be read: $($existing.Status) $($existing.Error)" }
if ($existing.Data.totalCount -gt 0 -and -not $Force) {
    "The environment already holds $($existing.Data.totalCount) claims. Run again with -Force to add $Count more."
    return
}

$policies = @((Invoke-Api 'GET' '/api/policies/search?pageSize=100' $tokens.Handler $null).Data.items)
$causeCodes = @((Invoke-Api 'GET' '/api/reference/cause-of-loss-codes' $tokens.Handler $null).Data | ForEach-Object { $_.code })
if ($policies.Count -eq 0 -or $causeCodes.Count -eq 0) { throw 'The policies or the cause codes could not be read.' }

# --- Data pools ----------------------------------------------------------------------------------------------

$firstNames = 'Ada', 'Boris', 'Clara', 'Dmitri', 'Elena', 'Felix', 'Greta', 'Hugo', 'Irina', 'Jonas', 'Karina', 'Leon', 'Mira', 'Nikolai', 'Olga', 'Pavel', 'Rita', 'Stefan', 'Tamara', 'Viktor'
$lastNames = 'Stone', 'Novak', 'Keller', 'Moreau', 'Petrov', 'Silva', 'Lindqvist', 'Rossi', 'Haas', 'Costa', 'Weber', 'Nowak', 'Duarte', 'Fischer', 'Larsen', 'Marek', 'Orlov', 'Quinn', 'Tanaka', 'Voss'
$companies = 'Northwind Logistics', 'Bluebird Foods', 'Harbor Print Works', 'Summit Builders', 'Greenfield Farms', 'Atlas Freight', 'Cedar Dental', 'Delta Plastics', 'Evergreen Hotels', 'Falcon Couriers'
$locations = 'Warehouse 4', 'Main street 12', 'Kitchen', 'Parking lot B', 'Ring road exit 8', 'Basement archive', 'Loading dock', 'Office floor 3', 'Depot north', 'Customer site'
$notes = 'Called the claimant, documents promised this week.', 'Expert visit planned for next Tuesday.', 'Waiting for the repair estimate.', 'Police report requested.', 'Photos received, damage confirmed.', 'Liability under review with the insured.'

# Description pools and asset types by cause of loss; an unknown code falls back to the "other" entry.
$causeInfo = @{
    'COL-FIRE'      = @{ Assets = 'Property', 'Equipment'; Text = 'Fire started in the electrical cabinet', 'Kitchen fire spread to the ceiling', 'Overheated machine caught fire' }
    'COL-FLOOD'     = @{ Assets = 'Property'; Text = 'Flooding after heavy rain in the basement', 'Burst pipe flooded two floors', 'Drain blocked and water entered the ground floor' }
    'COL-THEFT'     = @{ Assets = 'Property', 'Equipment', 'Vehicle'; Text = 'Break-in overnight, stock removed', 'Tools stolen from the van', 'Laptops taken from the office' }
    'COL-VEH-COL'   = @{ Assets = 'Vehicle'; Text = 'Collision at a junction in heavy rain', 'Rear-end collision on the ring road', 'Reversing accident in the depot' }
    'COL-VEH-COMP'  = @{ Assets = 'Vehicle'; Text = 'Windscreen cracked by a stone', 'Hail damage to the bodywork', 'Parked vehicle hit by a falling branch' }
    'COL-LIAB'      = @{ Assets = 'Other', 'Person'; Text = 'Visitor slipped on a wet floor', 'Customer property damaged during delivery', 'Contractor injured on the premises' }
    'COL-EQUIP'     = @{ Assets = 'Equipment'; Text = 'Equipment stopped working after a power surge', 'Compressor failed during operation', 'Control unit damaged by overheating' }
    'COL-WIND'      = @{ Assets = 'Property'; Text = 'Storm damage to the roof and windows', 'Fence and shed destroyed by wind', 'Roof tiles lost in a gale' }
    'COL-INJURY'    = @{ Assets = 'Person'; Text = 'Employee injured while loading a pallet', 'Fall from a ladder in the warehouse', 'Hand injury at a cutting machine' }
    'COL-OTHER'     = @{ Assets = 'Other', 'Property'; Text = 'Damage of unknown origin found on arrival', 'Stored goods damaged in transit', 'Contamination of a stock room' }
}
$assetText = @{
    Vehicle   = 'Delivery van', 'Company car', 'Forklift', 'Pickup truck'
    Property  = 'Office building', 'Warehouse', 'Shop unit', 'Storage hall'
    Person    = 'Driver', 'Warehouse operator', 'Visitor'
    Equipment = 'Industrial oven', 'Packing line', 'Compressor', 'Server rack'
    Other     = 'Stored goods', 'Customer shipment', 'Signage'
}
$reserveRanges = @{
    Indemnity              = @(2000, 180000)
    Expense                = @(300, 15000)
    Alae                   = @(200, 9000)
    SubrogationRecoverable = @(500, 30000)
}
$reasons = 'Expert report received', 'Repair estimate updated', 'Legal opinion obtained', 'Settlement discussed', 'Invoice received', 'Estimate corrected after inspection'

$pdfPath = Join-Path $env:TEMP 'demo-claim-document.pdf'
[System.IO.File]::WriteAllText($pdfPath, "%PDF-1.4`n1 0 obj<</Type/Catalog>>endobj`ntrailer<</Root 1 0 R>>`n%%EOF`n")

$now = (Get-Date).ToUniversalTime()
$today = $now.Date

# --- Building blocks -----------------------------------------------------------------------------------------

function New-Parties {
    $parties = @()
    if (Get-Chance 0.2) {
        $parties += @{ Role = 'Claimant'; Type = 'Company'; CompanyName = (Get-OneOf $companies); Email = 'office@example.com' }
    }
    else {
        $first = Get-OneOf $firstNames
        $last = Get-OneOf $lastNames
        $phone = '+4850' + ('{0:D7}' -f (Get-Int 1000000 9999999))
        $parties += @{ Role = 'Claimant'; Type = 'Person'; FirstName = $first; LastName = $last; Email = "$($first.ToLower()).$($last.ToLower())@example.com"; Phone = $phone }
    }

    $extra = [int](Get-Weighted @{ '0' = 45; '1' = 30; '2' = 18; '3' = 7 })
    for ($index = 0; $index -lt $extra; $index++) {
        $role = Get-OneOf @('Insured', 'ThirdParty', 'Witness', 'Attorney')
        if ($role -eq 'Attorney' -or (Get-Chance 0.25)) {
            $parties += @{ Role = $role; Type = 'Company'; CompanyName = (Get-OneOf $companies) }
        }
        else {
            $parties += @{ Role = $role; Type = 'Person'; FirstName = (Get-OneOf $firstNames); LastName = (Get-OneOf $lastNames) }
        }
    }

    return $parties
}

function New-RiskObjects([string]$Cause) {
    $info = if ($causeInfo.ContainsKey($Cause)) { $causeInfo[$Cause] } else { $causeInfo['COL-OTHER'] }
    $objects = @()
    $count = [int](Get-Weighted @{ '1' = 62; '2' = 28; '3' = 10 })
    for ($index = 0; $index -lt $count; $index++) {
        $type = if ($index -eq 0) { Get-OneOf $info.Assets } else { Get-OneOf @('Vehicle', 'Property', 'Person', 'Equipment', 'Other') }
        $objects += @{
            AssetType         = $type
            AssetDescription  = (Get-OneOf $assetText[$type])
            IsPrimary         = ($index -eq 0)
            DamageDescription = 'See the loss description'
        }
    }

    return $objects
}

# The coverage of a policy is checked on the loss date; about one claim in twelve gets a policy that does not cover it,
# which raises a policy-period warning.
function Select-Policy([datetime]$LossDate, [bool]$IsOutsidePeriod) {
    $covering = @($policies | Where-Object { [datetime]$_.effectiveDate -le $LossDate.Date -and [datetime]$_.expirationDate -ge $LossDate.Date })
    $others = @($policies | Where-Object { $covering -notcontains $_ })
    if ($IsOutsidePeriod -and $others.Count -gt 0) { return Get-OneOf $others }
    if ($covering.Count -gt 0) { return Get-OneOf $covering }

    return Get-OneOf $policies
}

function Get-StatusFor([int]$AgeDays) {
    if ($AgeDays -gt 150) { return Get-Weighted @{ Draft = 10; Open = 14; UnderInvestigation = 12; PendingPayment = 8; Closed = 34; Withdrawn = 14; Reopened = 8 } }
    if ($AgeDays -gt 60) { return Get-Weighted @{ Draft = 15; Open = 24; UnderInvestigation = 18; PendingPayment = 14; Closed = 16; Withdrawn = 8; Reopened = 5 } }
    if ($AgeDays -gt 14) { return Get-Weighted @{ Draft = 22; Open = 30; UnderInvestigation = 18; PendingPayment = 12; Closed = 8; Withdrawn = 6; Reopened = 4 } }

    return Get-Weighted @{ Draft = 38; Open = 38; UnderInvestigation = 12; PendingPayment = 6; Closed = 3; Withdrawn = 3 }
}

# Who may approve a row: nobody approves their own, supervisors up to 100,000 and managers above.
function Get-Approver([string]$SubmitterRole, [decimal]$Amount) {
    if ($SubmitterRole -eq 'Manager') {
        if ([math]::Abs($Amount) -gt 100000) { return $null }

        return 'Supervisor'
    }
    if ($SubmitterRole -eq 'Supervisor') { return 'Manager' }
    if ([math]::Abs($Amount) -gt 100000) { return 'Manager' }

    return 'Supervisor'
}

# Decides what happens to a submitted row and returns the approved amount it adds to the balance.
function Resolve-Row($Claim, $Row, [bool]$MustApprove) {
    $amount = [decimal]$Row.amount
    if ($Row.approvalStatus -in @('AutoApproved', 'Approved')) { return $amount }

    $approver = Get-Approver $Claim.SubmitterRole $amount
    $fate = if ($MustApprove) { 'Approve' }
    elseif ($Claim.IsSettled) { Get-Weighted @{ Approve = 78; Reject = 14; Retract = 8 } }
    else { Get-Weighted @{ Approve = 58; Reject = 12; Wait = 18; Retract = 12 } }
    $path = "/api/claims/$($Claim.Id)/reserves/$($Row.id)"
    if ($fate -eq 'Retract') {
        [void](Confirm-Step "$($Claim.Label) retract $amount" (Invoke-Api 'POST' "$path/retract" $Claim.SubmitterToken $null))

        return 0
    }
    if ($null -eq $approver -or $fate -eq 'Wait') { return 0 }

    if ($fate -eq 'Approve') {
        $approved = Confirm-Step "$($Claim.Label) approve $amount" (Invoke-Api 'POST' "$path/approve" $tokens[$approver] $null)

        return $(if ($approved) { $amount } else { 0 })
    }

    [void](Confirm-Step "$($Claim.Label) reject $amount" (Invoke-Api 'POST' "$path/reject" $tokens[$approver] @{ rejectionReason = 'Not supported by the file' }))

    return 0
}

function Add-Reserves($Claim) {
    $count = [int](Get-Weighted @{ '0' = 10; '1' = 46; '2' = 30; '3' = 14 })
    if ($Claim.MustHaveApproved -and $count -eq 0) { $count = 1 }
    $types = @('Indemnity', 'Expense', 'Alae', 'SubrogationRecoverable')
    $cap = if ($Claim.SubmitterRole -eq 'Manager') { 60000 } else { 1000000 }
    $hasApproved = $false

    for ($index = 0; $index -lt $count; $index++) {
        $type = if ($index -eq 0 -and (Get-Chance 0.85)) { 'Indemnity' } else { $types[($index + (Get-Int 0 3)) % $types.Count] }
        $range = $reserveRanges[$type]
        $amount = [math]::Min((Get-Amount $range[0] $range[1]), $cap)
        $body = @{ TransactionType = 'Add'; Amount = $amount; ChangeReason = (Get-OneOf $reasons); ComponentType = $type }
        $added = Invoke-Api 'POST' "/api/claims/$($Claim.Id)/reserves" $Claim.SubmitterToken $body
        if (-not (Confirm-Step "$($Claim.Label) add $type $amount" $added)) { continue }

        $row = $added.Data.data
        $balance = Resolve-Row $Claim $row ($Claim.MustHaveApproved -and -not $hasApproved)
        if ($balance -gt 0) { $hasApproved = $true }

        $followUps = [int](Get-Weighted @{ '0' = 34; '1' = 34; '2' = 20; '3' = 12 })
        for ($step = 0; $step -lt $followUps -and $balance -gt 300; $step++) {
            $kind = Get-Weighted @{ Raise = 50; Lower = 28; Reverse = 22 }
            if ($kind -eq 'Raise') { $change = [math]::Round($balance * (0.1 + 0.3 * $script:Random.NextDouble()) / 50) * 50; $transaction = 'Adjust' }
            elseif ($kind -eq 'Lower') { $change = -[math]::Round($balance * (0.1 + 0.4 * $script:Random.NextDouble()) / 50) * 50; $transaction = 'Adjust' }
            else { $change = -[math]::Round($balance * (0.2 + 0.5 * $script:Random.NextDouble()) / 50) * 50; $transaction = 'Reverse' }
            if ([math]::Abs($change) -lt 50) { continue }
            if ($change -gt 0 -and $change -gt ($cap - $balance)) { continue }

            $followBody = @{ TransactionType = $transaction; Amount = [decimal]$change; ChangeReason = (Get-OneOf $reasons); ComponentId = $row.reserveComponentId }
            $follow = Invoke-Api 'POST' "/api/claims/$($Claim.Id)/reserves" $Claim.SubmitterToken $followBody
            if (-not (Confirm-Step "$($Claim.Label) $transaction $change" $follow)) { continue }
            $balance += Resolve-Row $Claim $follow.Data.data $false
        }
    }
}

# A decision that the rules must refuse (the approval tier or the claim limit); a refusal is the expected result.
function Test-Refusal($Claim, $Row, [string]$Role) {
    $result = Invoke-Api 'POST' "/api/claims/$($Claim.Id)/reserves/$($Row.id)/approve" $tokens[$Role] $null
    $text = "$($Claim.Label) $Role approve $($Row.amount) -> $($result.Status) $($result.Error)"
    if ($result.Ok) { [void]$script:Failures.Add("$text (a refusal was expected)") } else { [void]$script:ExpectedRefusals.Add($text) }
}

function Approve-Row($Claim, $Row, [string]$Role) {
    if ($Row.approvalStatus -in @('AutoApproved', 'Approved')) { return }

    [void](Confirm-Step "$($Claim.Label) $Role approve $($Row.amount)" (Invoke-Api 'POST' "/api/claims/$($Claim.Id)/reserves/$($Row.id)/approve" $tokens[$Role] $null))
}

function Submit-Row($Claim, [string]$Type, [decimal]$Amount) {
    $body = @{ TransactionType = 'Add'; Amount = $Amount; ChangeReason = (Get-OneOf $reasons); ComponentType = $Type }
    $added = Invoke-Api 'POST' "/api/claims/$($Claim.Id)/reserves" $Claim.SubmitterToken $body
    if (-not (Confirm-Step "$($Claim.Label) add $Type $Amount" $added)) { return $null }

    return $added.Data.data
}

# Claims that reach the 10 million claim limit: the rows above 100,000 are approved by the manager, and the last rows
# would take the total over the limit, so their approval is refused and they stay waiting.
function Add-LimitReserves($Claim) {
    $variants = @(
        @{ Approved = @(4000000, 3500000, 2500000); Refused = @(100) },
        @{ Approved = @(6000000, 3200000, 750000); Refused = @(100000) },
        @{ Approved = @(2000000, 1500000, 400000, 6000000); Refused = @(200000) }
    )
    $variant = $variants[$script:LimitClaims % $variants.Count]
    $script:LimitClaims++

    foreach ($amount in $variant.Approved) {
        $row = Submit-Row $Claim 'Indemnity' $amount
        if ($null -ne $row) { Approve-Row $Claim $row 'Manager' }
    }
    foreach ($amount in $variant.Refused) {
        $row = Submit-Row $Claim 'Indemnity' $amount
        if ($null -ne $row) { Test-Refusal $Claim $row 'Manager' }
    }
}

# Claims at the edges of the approval tiers: up to 10,000 approves itself, up to 100,000 needs a supervisor, above that
# a manager; a supervisor refused on 100,050 shows the tier, and a row left for a manager shows a waiting one.
function Add-TierReserves($Claim) {
    $approver = if ($Claim.SubmitterRole -eq 'Supervisor') { 'Manager' } else { 'Supervisor' }

    $row = Submit-Row $Claim 'Indemnity' 10000
    if ($null -ne $row) { Approve-Row $Claim $row $approver }
    $row = Submit-Row $Claim 'Expense' 10050
    if ($null -ne $row) { Approve-Row $Claim $row $approver }
    $row = Submit-Row $Claim 'Indemnity' 100000
    if ($null -ne $row) { Approve-Row $Claim $row $approver }
    $row = Submit-Row $Claim 'Alae' 100050
    if ($null -ne $row) {
        if ($Claim.SubmitterRole -eq 'Handler') { Test-Refusal $Claim $row 'Supervisor' }
        Approve-Row $Claim $row 'Manager'
    }
    [void](Submit-Row $Claim 'SubrogationRecoverable' 100050)
}

function Send-Documents($Claim) {
    $total = [int](Get-Weighted @{ '1' = 70; '2' = 22; '3' = 8 })
    $types = 'Police report', 'Photos', 'Repair estimate', 'Invoice', 'Expert report'
    for ($index = 0; $index -lt $total; $index++) {
        $url = "$script:ApiUrl/api/claims/$($Claim.Id)/documents"
        $code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 120 -X POST $url -H "Authorization: Bearer $($Claim.SubmitterToken)" `
            -F "file=@$pdfPath;type=application/pdf" -F "documentType=$(Get-OneOf $types)" -F 'notes=Seeded demo document'
        if ($code -ne '201') { [void]$script:Failures.Add("$($Claim.Label) upload -> $code") }
    }
}

# A warning on the policy period is acknowledged and a too short description is waived by a supervisor, so the claim can
# open with those issues resolved. The missing risk object warning is left raised: it never blocks a move.
function Resolve-Issues($Claim) {
    $detail = Invoke-Api 'GET' "/api/claims/$($Claim.Id)" $Claim.SubmitterToken $null
    if (-not (Confirm-Step "$($Claim.Label) read issues" $detail)) { return }

    foreach ($issue in @($detail.Data.issues | Where-Object { $_.state -eq 'Raised' })) {
        $path = "/api/claims/$($Claim.Id)/issues/$($issue.id)"
        if ($issue.code -eq 'S:BR-C-02') {
            [void](Confirm-Step "$($Claim.Label) acknowledge $($issue.code)" (Invoke-Api 'POST' "$path/acknowledge" $Claim.SubmitterToken $null))
        }
        elseif ($issue.code -in @('S:BR-C-07', 'S:BR-C-01')) {
            [void](Confirm-Step "$($Claim.Label) waive $($issue.code)" (Invoke-Api 'POST' "$path/waive" $tokens.Supervisor $null))
        }
    }
}

# Closing and withdrawing are refused while an approved row is still being posted, so each is retried after a pause.
function Move-Claim($Claim, [hashtable]$Body, [string]$Token) {
    $result = $null
    $attempts = if ($Body.TargetStatus -in @('Closed', 'Withdrawn')) { 3 } else { 1 }
    for ($attempt = 1; $attempt -le $attempts; $attempt++) {
        $result = Invoke-Api 'PUT' "/api/claims/$($Claim.Id)/status" $Token $Body
        if ($result.Ok) { return $true }
        if ($attempt -lt $attempts) { Start-Sleep -Seconds 15 }
    }

    return (Confirm-Step "$($Claim.Label) move to $($Body.TargetStatus)" $result)
}

# --- Phase 1: create the claims in loss-date order, open them and set up the reserves ------------------------

$offsets = @(1..$Count | ForEach-Object { Get-Int 2 $WindowDays } | Sort-Object -Descending)
$roles = @{ Handler = 'Handler'; Supervisor = 'Supervisor'; Manager = 'Manager' }
$claims = New-Object System.Collections.ArrayList
$shuffled = @(0..($Count - 1) | Sort-Object { $script:Random.Next() })
$limitIndexes = if ($Count -ge 20) { @($shuffled[0..2]) } else { @() }
$tierIndexes = if ($Count -ge 20) { @($shuffled[3..7]) } else { @() }

for ($index = 0; $index -lt $Count; $index++) {
    $daysAgo = $offsets[$index]
    $lossDate = $today.AddDays(-$daysAgo).AddHours((Get-Int 6 20)).AddMinutes((Get-Int 0 59))
    $reportDate = $lossDate.AddDays((Get-Int 0 10))
    if ($reportDate -gt $now.AddHours(-1)) { $reportDate = $now.AddHours(-1) }

    $special = if ($limitIndexes -contains $index) { 'Limit' } elseif ($tierIndexes -contains $index) { 'Tier' } else { '' }
    $flaw = if ($special) { 'None' } else { Get-Weighted @{ None = 64; Outside = 10; Short = 7; FutureLoss = 5; UnknownCause = 4; NoClaimant = 4 } }
    $isFixable = $flaw -in @('Outside', 'Short', 'FutureLoss')
    $isStuck = $flaw -in @('UnknownCause', 'NoClaimant')
    if ($flaw -eq 'FutureLoss') {
        $lossDate = $now.AddDays((Get-Int 1 5))
        $reportDate = $now.AddHours(-1)
    }
    $target = if ($special) { Get-OneOf @('Open', 'UnderInvestigation') }
    elseif ($isStuck -or ($isFixable -and (Get-Chance 0.4))) { 'Draft' }
    else { Get-StatusFor $daysAgo }
    $policy = Select-Policy $lossDate ($flaw -eq 'Outside')
    $cause = if ($flaw -eq 'UnknownCause') { 'COL-UNKNOWN' } else { Get-OneOf $causeCodes }
    $info = if ($causeInfo.ContainsKey($cause)) { $causeInfo[$cause] } else { $causeInfo['COL-OTHER'] }
    $ownerRole = if ($special) { Get-OneOf @('Handler', 'Supervisor') } else { Get-Weighted @{ Handler = 52; Supervisor = 26; Manager = 22 } }
    $label = "[$($index + 1)/$Count $target]"
    $description = if ($flaw -eq 'Short') { Get-OneOf @('Water damage', 'Rear-end collision', 'Stolen laptop', 'Fire in the shed') } else { Get-OneOf $info.Text }
    $riskObjects = @()
    if (-not (Get-Chance 0.12)) { $riskObjects = @(New-RiskObjects $cause) }
    $parties = @(New-Parties)
    if ($flaw -eq 'NoClaimant') { $parties = @() }

    $body = @{
        PolicyId    = $policy.id
        ReportDate  = (Format-Utc $reportDate)
        Loss        = @{
            LossDate            = (Format-Utc $lossDate)
            Description         = $description
            CauseOfLossCode     = $cause
            Location            = (Get-OneOf $locations)
            EstimatedLossAmount = (Get-Amount 800 220000)
        }
        Parties     = $parties
        RiskObjects = $riskObjects
        Reserves    = @()
    }
    if ($target -eq 'Draft' -and (Get-Chance 0.4)) {
        $body.Reserves = @(@{ TransactionType = 'Add'; Amount = (Get-Amount 500 20000); ChangeReason = 'Initial reserve'; ComponentType = 'Indemnity' })
    }

    $created = Invoke-Api 'POST' '/api/claims' $tokens.Handler $body
    if (-not (Confirm-Step "$label create" $created)) { continue }

    $claim = [pscustomobject]@{
        Id               = $created.Data.data.id
        Number           = $created.Data.data.claimNumber
        Label            = $label
        Target           = $target
        OwnerRole        = $ownerRole
        SubmitterRole    = $ownerRole
        SubmitterToken   = $tokens[$ownerRole]
        IsSettled        = ($target -in @('Closed', 'Withdrawn', 'Reopened'))
        MustHaveApproved = ($target -in @('PendingPayment', 'Closed', 'Reopened'))
    }
    Write-Host "$label $($claim.Number) $($lossDate.ToString('yyyy-MM-dd')) handled by $ownerRole"

    if ($ownerRole -ne 'Handler') {
        [void](Confirm-Step "$label assign handler" (Invoke-Api 'PUT' "/api/claims/$($claim.Id)/handler" $tokens.Handler @{ handlerId = $userIdByRole[$ownerRole] }))
    }

    if (Get-Chance 0.4) {
        [void](Confirm-Step "$label notes" (Invoke-Api 'PUT' "/api/claims/$($claim.Id)/notes" $claim.SubmitterToken @{ notes = (Get-OneOf $notes) }))
    }

    if ($target -ne 'Draft') {
        if ($isFixable) { Resolve-Issues $claim }
        if (-not (Confirm-Step "$label open" (Invoke-Api 'PUT' "/api/claims/$($claim.Id)/status" $claim.SubmitterToken @{ TargetStatus = 'Open' }))) { continue }
        if ($special -eq 'Limit') { Add-LimitReserves $claim }
        elseif ($special -eq 'Tier') { Add-TierReserves $claim }
        else { Add-Reserves $claim }
        if (Get-Chance 0.35) { Send-Documents $claim }
    }

    [void]$claims.Add($claim)
}

Remove-Item $pdfPath -ErrorAction SilentlyContinue

# --- Phase 2: the later status moves, after the background posting of the approved rows ---------------------

$moves = @($claims | Where-Object { $_.Target -notin @('Draft', 'Open') })
if ($moves.Count -gt 0) {
    Write-Host "Waiting 30 seconds for the GL posting jobs before the status moves (closing needs every approved row posted)."
    Start-Sleep -Seconds 30
}

foreach ($claim in $moves) {
    $token = $claim.SubmitterToken
    switch ($claim.Target) {
        'UnderInvestigation' {
            [void](Move-Claim $claim @{ TargetStatus = 'UnderInvestigation'; Note = 'Expert opinion requested' } $token)
        }
        'PendingPayment' {
            [void](Move-Claim $claim @{ TargetStatus = 'PendingPayment'; IsLiabilityConfirmed = $true; Note = 'Liability confirmed' } $token)
        }
        'Closed' {
            if (Get-Chance 0.4) {
                [void](Move-Claim $claim @{ TargetStatus = 'PendingPayment'; IsLiabilityConfirmed = $true; Note = 'Liability confirmed' } $token)
            }
            [void](Move-Claim $claim @{ TargetStatus = 'Closed'; ReasonCode = 'SETTLED'; ReasonText = 'Settled with the claimant'; Note = 'Closed after settlement' } $token)
        }
        'Withdrawn' {
            if (Get-Chance 0.3) {
                [void](Move-Claim $claim @{ TargetStatus = 'UnderInvestigation'; Note = 'Checking the policy terms' } $token)
            }
            [void](Move-Claim $claim @{ TargetStatus = 'Withdrawn'; ReasonCode = 'CLAIMANT_REQUEST'; ReasonText = 'The claimant withdrew the claim'; Note = 'Withdrawn at the claimant request' } $token)
        }
        'Reopened' {
            if (Move-Claim $claim @{ TargetStatus = 'Closed'; ReasonCode = 'SETTLED'; ReasonText = 'Settled with the claimant'; Note = 'Closed after settlement' } $token) {
                $reopen = @{ TargetStatus = 'Reopened'; ReasonCode = 'NEW_EVIDENCE'; ReasonText = 'New documents were received'; Note = 'Reopened for new evidence' }
                if ((Move-Claim $claim $reopen $tokens.Manager) -and (Get-Chance 0.5)) {
                    [void](Move-Claim $claim @{ TargetStatus = 'Open'; Note = 'Back in handling' } $token)
                }
            }
        }
    }
}

# --- Summary -------------------------------------------------------------------------------------------------

''
'Claims by status now:'
foreach ($status in 'Draft', 'Open', 'UnderInvestigation', 'PendingPayment', 'Closed', 'Reopened', 'Withdrawn') {
    $page = Invoke-Api 'GET' "/api/claims?status=$status&pageSize=1" $tokens.Handler $null
    "  {0,-20} {1}" -f $status, $page.Data.totalCount
}

''
'Claims by handler:'
foreach ($role in 'Handler', 'Supervisor', 'Manager') {
    "  {0,-20} {1}" -f $role, @($claims | Where-Object { $_.OwnerRole -eq $role }).Count
}

''
'Data summary (read back from the API):'
$issueLines = New-Object System.Collections.ArrayList
$rowStates = New-Object System.Collections.ArrayList
$totals = New-Object System.Collections.ArrayList
foreach ($claim in $claims) {
    $detail = Invoke-Api 'GET' "/api/claims/$($claim.Id)" $tokens.Handler $null
    if (-not $detail.Ok) { continue }
    foreach ($issue in @($detail.Data.issues)) { [void]$issueLines.Add("$($issue.severity) $($issue.code) $($issue.state)") }
    foreach ($row in @($detail.Data.reserves.history)) { [void]$rowStates.Add("$($row.approvalStatus)") }
    [void]$totals.Add([decimal]$detail.Data.reserves.approvedTotal)
}
'  Issues (severity, code, state):'
foreach ($group in ($issueLines | Group-Object | Sort-Object Name)) { "    {0,-48} {1}" -f $group.Name, $group.Count }
'  Reserve rows by approval state:'
foreach ($group in ($rowStates | Group-Object | Sort-Object Name)) { "    {0,-24} {1}" -f $group.Name, $group.Count }
$withReserves = @($totals | Where-Object { $_ -gt 0 })
'  Approved total per claim: {0} claims with a balance, sum {1:N0}, largest {2:N0}, above 100,000: {3}, at least 10,000,000: {4}' -f `
    $withReserves.Count, ($withReserves | Measure-Object -Sum).Sum, ($withReserves | Measure-Object -Maximum).Maximum, `
    @($withReserves | Where-Object { $_ -gt 100000 }).Count, @($withReserves | Where-Object { $_ -ge 10000000 }).Count

''
"Expected refusals (approval tier and claim limit checks): $($script:ExpectedRefusals.Count)"
foreach ($line in ($script:ExpectedRefusals | Select-Object -First 12)) { "  $line" }

''
"Refused or failed steps: $($script:Failures.Count)"
foreach ($failure in ($script:Failures | Select-Object -First 25)) { "  $failure" }
if ($script:Failures.Count -gt 25) { "  ... and $($script:Failures.Count - 25) more" }
