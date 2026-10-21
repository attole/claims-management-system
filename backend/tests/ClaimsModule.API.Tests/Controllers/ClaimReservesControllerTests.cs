using System.Net;
using System.Net.Http.Json;
using ClaimsModule.API.Controllers;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.ReserveComponents.Dtos;
using ClaimsModule.Application.ReserveComponents.History.Dtos;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed class ClaimReservesControllerTests(ApiFixture database) : IDisposable
{
    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-reserves")]
    public async Task Get_ClaimWithoutReserves_ReturnsAnEmptySummary()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.GetAsync($"/api/claims/{claimId}/reserves");

        var reserves = await response.Content.ReadFromJsonAsync<ClaimReservesDto>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        reserves!.Components.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task Submit_UnknownTransactionTypeName_Answers422()
    {
        using var client = CreateClient(Guid.NewGuid());
        var body = new { TransactionType = "Bogus", Amount = 100, ChangeReason = "Why", ComponentType = "Indemnity" };

        var response = await client.PostAsJsonAsync($"/api/claims/{Guid.NewGuid()}/reserves", body);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserves")]
    public async Task Submit_ToAClaimOfAnotherOrganisation_Answers404()
    {
        var (claimId, _) = await _factory.SeedClaimAsync(Guid.NewGuid());
        using var client = CreateClient(Guid.NewGuid());
        var body = new { TransactionType = "Add", Amount = 100, ChangeReason = "Initial", ComponentType = "Indemnity" };

        var response = await client.PostAsJsonAsync($"/api/claims/{claimId}/reserves", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiTheory]
    [Trait("Rule", "S:API-POST-reserve-approve")]
    [Trait("Rule", "S:API-POST-reserve-retract")]
    [InlineData("approve")]
    [InlineData("retract")]
    [InlineData("retry-posting")]
    public async Task Decide_UnknownReserveHistory_Answers404(string action)
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsync($"/api/claims/{claimId}/reserves/{Guid.NewGuid()}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserve-reject")]
    public async Task Reject_UnknownReserveHistory_Answers404()
    {
        var organisationId = Guid.NewGuid();
        var (claimId, _) = await _factory.SeedClaimAsync(organisationId);
        using var client = CreateClient(organisationId);

        var response = await client.PostAsJsonAsync(
            $"/api/claims/{claimId}/reserves/{Guid.NewGuid()}/reject",
            new { RejectionReason = "Not covered" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserves")]
    public async Task Submit_AmountWithinTheAutoApprovalLimit_Answers201AsAutoApproved()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        using var handler = CreateSeedClient(ApiFactory.HandlerId, "Handler");

        var response = await SubmitAsync(handler, claimId, 5_000);

        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ReserveHistoryDto>>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        envelope!.Data.ApprovalStatus.Should().Be("AutoApproved");
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserves")]
    public async Task Submit_AmountAboveTheAutoApprovalLimit_Answers201AsPending()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        using var handler = CreateSeedClient(ApiFactory.HandlerId, "Handler");

        var response = await SubmitAsync(handler, claimId, 50_000);

        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ReserveHistoryDto>>();
        envelope!.Data.ApprovalStatus.Should().Be("PendingApproval");
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserve-approve")]
    public async Task Approve_ASupervisorApprovingAnotherUsersReserve_Answers200AsApproved()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        using var handler = CreateSeedClient(ApiFactory.HandlerId, "Handler");
        using var supervisor = CreateSeedClient(ApiFactory.SupervisorId, "Supervisor");
        var historyId = await SubmitPendingAsync(handler, claimId);

        var response = await supervisor.PostAsync($"/api/claims/{claimId}/reserves/{historyId}/approve", null);

        var envelope = await ReadStatusAsync(response);
        envelope!.Data.NewStatus.Should().Be("Approved");
    }

    [ApiFact]
    [Trait("Rule", "S:BR-R-03")]
    public async Task Approve_TheSubmitterApprovingTheirOwnReserve_Answers422()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        using var supervisor = CreateSeedClient(ApiFactory.SupervisorId, "Supervisor");
        var historyId = await SubmitPendingAsync(supervisor, claimId);

        var response = await supervisor.PostAsync($"/api/claims/{claimId}/reserves/{historyId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:BR-R-02")]
    public async Task Approve_AHandler_IsRefused()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        using var supervisor = CreateSeedClient(ApiFactory.SupervisorId, "Supervisor");
        using var handler = CreateSeedClient(ApiFactory.HandlerId, "Handler");
        var historyId = await SubmitPendingAsync(supervisor, claimId);

        var response = await handler.PostAsync($"/api/claims/{claimId}/reserves/{historyId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserve-reject")]
    public async Task Reject_ASupervisorWithAReason_Answers200AsRejected()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        using var handler = CreateSeedClient(ApiFactory.HandlerId, "Handler");
        using var supervisor = CreateSeedClient(ApiFactory.SupervisorId, "Supervisor");
        var historyId = await SubmitPendingAsync(handler, claimId);

        var response = await supervisor.PostAsJsonAsync(
            $"/api/claims/{claimId}/reserves/{historyId}/reject",
            new { RejectionReason = "Not supported by the file" });

        var envelope = await ReadStatusAsync(response);
        envelope!.Data.NewStatus.Should().Be("Rejected");
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserve-retract")]
    public async Task Retract_TheSubmitterOfAPendingReserve_Answers200AsCancelled()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        using var handler = CreateSeedClient(ApiFactory.HandlerId, "Handler");
        var historyId = await SubmitPendingAsync(handler, claimId);

        var response = await handler.PostAsync($"/api/claims/{claimId}/reserves/{historyId}/retract", null);

        var envelope = await ReadStatusAsync(response);
        envelope!.Data.NewStatus.Should().Be("Cancelled");
    }

    [ApiFact]
    [Trait("Rule", "S:API-POST-reserve-retract")]
    public async Task Retract_SomeoneElseThanTheSubmitter_Answers422()
    {
        var claimId = await _factory.SeedOpenClaimAsync();
        using var handler = CreateSeedClient(ApiFactory.HandlerId, "Handler");
        using var supervisor = CreateSeedClient(ApiFactory.SupervisorId, "Supervisor");
        var historyId = await SubmitPendingAsync(handler, claimId);

        var response = await supervisor.PostAsync($"/api/claims/{claimId}/reserves/{historyId}/retract", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    private static async Task<HttpResponseMessage> SubmitAsync(HttpClient client, Guid claimId, decimal amount)
    {
        var body = new { TransactionType = "Add", Amount = amount, ChangeReason = "Why", ComponentType = "Indemnity" };

        return await client.PostAsJsonAsync($"/api/claims/{claimId}/reserves", body);
    }

    private static async Task<ApiControllerBase.CommandResponse<ReserveHistoryStatusDto>?> ReadStatusAsync(
        HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ReserveHistoryStatusDto>>();
    }

    private static async Task<Guid> SubmitPendingAsync(HttpClient client, Guid claimId)
    {
        var response = await SubmitAsync(client, claimId, 50_000);
        var envelope = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<ReserveHistoryDto>>();

        return envelope!.Data.Id;
    }

    private HttpClient CreateSeedClient(Guid userId, string role)
    {
        return _factory.CreateClientFor(userId, ApiFactory.SeedOrganisationId, role);
    }

    private HttpClient CreateClient(Guid organisationId)
    {
        return _factory.CreateClientFor(Guid.NewGuid(), organisationId, "Supervisor");
    }
}
