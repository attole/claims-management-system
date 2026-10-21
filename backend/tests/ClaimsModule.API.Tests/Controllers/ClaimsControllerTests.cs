using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClaimsModule.API.Controllers;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Audit.Dtos;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.ReserveComponents.History.Dtos;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ClaimsControllerTests(ApiFixture database) : IDisposable
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_WithoutParameters_ReturnsTheFirstPageOfTheDefaultSize()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/claims");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page.Should().BeEquivalentTo(new { Page = 1, PageSize = 25, TotalCount = 0 });
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_SearchByClaimNumber_ReturnsTheClaimOfTheOrganisation()
    {
        var organisationId = Guid.NewGuid();
        var (_, number) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Select(claim => claim.ClaimNumber).Should().Equal(number);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_AClaimWithACauseOfLoss_ReturnsItsCode()
    {
        var organisationId = Guid.NewGuid();
        var (_, number) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Select(claim => claim.CauseOfLossCode).Should().Equal(ApiFactory.SeedCauseOfLossCode);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_AClaimWithoutReserves_ReturnsAZeroReserveTotal()
    {
        var organisationId = Guid.NewGuid();
        var (_, number) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Select(claim => claim.TotalReserves).Should().Equal(0m);
    }

    [ApiFact]
    [Trait("Rule", "S:UI-List")]
    public async Task List_TwoClaimsOfTheOrganisation_CountsBoth()
    {
        var organisationId = Guid.NewGuid();
        await _factory.SeedClaimAsync(organisationId);
        await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync("/api/claims");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.TotalCount.Should().Be(2);
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task List_ClaimOfAnotherOrganisation_IsNotReturned()
    {
        var (_, number) = await _factory.SeedClaimAsync(Guid.NewGuid());
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync($"/api/claims?search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_RepeatedStatusParameters_FilterByEveryStatusGiven()
    {
        var organisationId = Guid.NewGuid();
        var (_, number) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?status=Open&status=Draft&search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Select(claim => claim.Status).Should().Equal("Draft");
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims")]
    public async Task List_StatusTheClaimDoesNotHave_ReturnsNothing()
    {
        var organisationId = Guid.NewGuid();
        var (_, number) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims?status=Closed&search={number}");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ClaimSummaryDto>>();
        page!.Items.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task List_UnknownStatusName_Answers422InTheErrorBody()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/claims?status=Bogus");

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body!.Type.Should().Be("ValidationError");
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task List_EndDateBeforeTheStartDate_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/claims?dateFrom=2026-03-01&dateTo=2026-02-01");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task List_PageSizeAboveTheCap_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/claims?pageSize=101");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:ST-Open-PendingPayment")]
    public async Task Journey_ApprovedReserveThenPendingPayment_ShowsInTheDetailAndTheAudit()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        var organisationId = ApiFactory.SeedOrganisationId;
        using var handler = _factory.CreateClientFor(ApiFactory.HandlerId, organisationId, "Handler");
        using var supervisor = _factory.CreateClientFor(ApiFactory.SupervisorId, organisationId, "Supervisor");
        var reserve = new
        {
            TransactionType = "Add",
            Amount = 50_000,
            ChangeReason = "Why",
            ComponentType = "Indemnity",
        };
        var submitted = await handler.PostAsJsonAsync($"/api/claims/{claimId}/reserves", reserve);
        var history = await submitted.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ReserveHistoryDto>>();
        await supervisor.PostAsync($"/api/claims/{claimId}/reserves/{history!.Data.Id}/approve", null);
        await handler.PutAsJsonAsync(
            $"/api/claims/{claimId}/status",
            new { TargetStatus = "PendingPayment", IsLiabilityConfirmed = true });

        var detail = await handler.GetFromJsonAsync<ClaimDetailDto>($"/api/claims/{claimId}");

        detail!.Claim.State.Should().Be("PendingPayment");
        detail.Reserves.ApprovedTotal.Should().Be(50_000);
        detail.RecentAudit.Should().NotBeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-claims")]
    public async Task Create_AnEmptyDraft_Answers201WithTheClaimInTheEnvelope()
    {
        using var client = CreateClient(await _factory.CreateOrganisationAsync());

        var response = await client.PostAsJsonAsync("/api/claims", new { ReportDate = DateTimeOffset.UtcNow });

        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ClaimDto>>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        envelope!.Data.State.Should().Be("Draft");
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task Create_RepeatedWithTheSameKey_ReturnsTheFirstClaim()
    {
        using var client = CreateClient(await _factory.CreateOrganisationAsync());
        var key = Guid.NewGuid().ToString();
        var body = new { ReportDate = DateTimeOffset.UtcNow };

        var first = await PostAsync(client, "/api/claims", key, body);
        var firstClaim = await first.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ClaimDto>>();

        var second = await PostAsync(client, "/api/claims", key, body);

        var secondClaim = await second.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ClaimDto>>();
        secondClaim!.Data.Id.Should().Be(firstClaim!.Data.Id);
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task Create_UnknownPartyRoleName_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());
        var body = new { Parties = new[] { new { Role = "Bogus", Type = "Person", FirstName = "Ada" } } };

        var response = await client.PostAsJsonAsync("/api/claims", body);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claim")]
    public async Task Get_ClaimOfTheOrganisation_ReturnsTheDetail()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, number) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims/{claimId}");

        var detail = await response.Content.ReadFromJsonAsync<ClaimDetailDto>();
        detail!.Claim.ClaimNumber.Should().Be(number);
    }

    [ApiFact]
    [Trait("Rule", "S:API-PUT-claims-status")]
    public async Task ChangeStatus_IncompleteDraftToOpen_Answers422WithTheBlockingErrors()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PutAsJsonAsync($"/api/claims/{claimId}/status", new { TargetStatus = "Open" });

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        body!.Errors.Should().NotBeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task ChangeStatus_UnknownStatusName_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());
        var body = new { TargetStatus = "Bogus" };

        var response = await client.PutAsJsonAsync($"/api/claims/{Guid.NewGuid()}/status", body);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task ChangeStatus_StatusGivenAsANumber_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());
        var body = new { TargetStatus = 1 };

        var response = await client.PutAsJsonAsync($"/api/claims/{Guid.NewGuid()}/status", body);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:BR-ST-03")]
    public async Task CheckStatus_IncompleteDraftToOpen_ReturnsTheBlockingErrors()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims/{claimId}/status-check?targetStatus=Open");

        var check = await response.Content.ReadFromJsonAsync<ClaimStatusCheckDto>(_json);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        check!.Errors.Should().NotBeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:BR-ST-03")]
    public async Task CheckStatus_IncompleteDraftToOpen_LeavesTheClaimInDraft()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var detail = await CheckThenGetAsync(client, claimId, "Open");

        detail.Claim.State.Should().Be("Draft");
    }

    [ApiFact]
    public async Task UpdateNotes_ADraftClaim_ReturnsTheClaimWithTheNotes()
    {
        var organisationId = await _factory.CreateOrganisationAsync();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);
        var body = new { Notes = "Called the insured" };

        var response = await client.PutAsJsonAsync($"/api/claims/{claimId}/notes", body);

        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ClaimDto>>();
        envelope!.Data.Notes.Should().Be("Called the insured");
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claims-audit")]
    public async Task ListAudit_ClaimOfTheOrganisation_ReturnsAPage()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims/{claimId}/audit");

        var page = await response.Content.ReadFromJsonAsync<PagedResult<AuditEntryDto>>();
        page.Should().BeEquivalentTo(new { Page = 1, PageSize = 25 });
    }

    private static async Task<ClaimDetailDto> CheckThenGetAsync(HttpClient client, Guid claimId, string target)
    {
        await client.GetAsync($"/api/claims/{claimId}/status-check?targetStatus={target}");

        return (await client.GetFromJsonAsync<ClaimDetailDto>($"/api/claims/{claimId}"))!;
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, string key, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", key);

        return await client.SendAsync(request);
    }

    private HttpClient CreateClient(Guid organisationId)
    {
        return _factory.CreateClientFor(Guid.NewGuid(), organisationId, "Handler");
    }
}
