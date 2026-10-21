using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ClaimsModule.API.Controllers;
using ClaimsModule.API.Errors;
using ClaimsModule.API.Pipeline.Idempotency;
using ClaimsModule.API.Tests.Support;
using ClaimsModule.Application.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.Tests.Controllers;

[Collection(ApiCollection.Name)]
public sealed partial class ApiControllerBaseTests(ApiFixture database) : IDisposable
{
    private static readonly string[] _writeMethods = ["POST", "PUT", "PATCH", "DELETE"];

    private static readonly string[] _singularSegments =
    [
        "api", "audit", "status", "status-check", "search", "reference", "approve", "reject", "retract",
        "retry-posting", "download", "dev", "token", "handler", "policy", "acknowledge", "waive",
    ];

    private readonly ApiFactory _factory = new(database);

    public void Dispose()
    {
        _factory.Dispose();
    }

    [ApiFact]
    public async Task QueryAsync_Success_ReturnsTheDataItselfWithoutAnEnvelope()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/test/read");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("7");
    }

    [ApiFact]
    public async Task QueryAsync_APagedResult_ReturnsItsItemsAndCounts()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/test/paged");

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"items\":[\"a\",\"b\"]").And.Contain("\"totalCount\":51");
        json.Should().Contain("\"page\":2").And.Contain("\"pageSize\":25");
    }

    [ApiFact]
    [Trait("Rule", "S:API-ErrorShape")]
    public async Task QueryAsync_Failure_ReturnsTheErrorBodyWithTheKindStatus()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/test/errors/Conflict");

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        body!.Type.Should().Be("ConflictError");
        body.Errors["Name"].Should().Equal("first message", "second message");
        body.Errors[ErrorBody.GeneralKey].Should().Equal("general message");
    }

    [ApiFact]
    public async Task CommandAsync_SuccessWithAWarning_ReturnsTheEnvelopeWithTheWarning()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-created", content: null);

        var body = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<int>>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body!.Data.Should().Be(1);
        body.Warnings.Should().Equal(new Warning("TEST_WARNING", "be careful", "Notes"));
    }

    [ApiFact]
    public async Task CommandAsync_SuccessWithoutWarnings_ReturnsTheEnvelopeWithAnEmptyList()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-plain", content: null);

        var body = await response.Content.ReadFromJsonAsync<ApiControllerBase.CommandResponse<int>>();
        body!.Warnings.Should().BeEmpty();
    }

    [ApiFact]
    public async Task CommandAsync_ActionDeclaringAnotherSuccessStatus_UsesThatStatus()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-accepted", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [ApiFact]
    public async Task CommandAsync_ActionWithoutASuccessStatus_Answers200()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-plain", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [ApiFact]
    public async Task CommandAsync_ActionDeclaringTwoSuccessStatuses_FailsLoudly()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/test/write-ambiguous", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClientFor(Guid.NewGuid(), Guid.NewGuid(), "Handler");
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task Routes_WithoutAToken_AllAnswer401()
    {
        using var client = _factory.CreateClient();

        var result = await RoutesNotAnswering(client, HttpStatusCode.Unauthorized, Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-GET-claim")]
    public async Task ClaimReadRoutes_ForAnUnknownClaim_AllAnswer404()
    {
        using var client = CreateClient();

        var result = await RoutesNotAnswering(client, HttpStatusCode.NotFound, Guid.NewGuid(), isReadOfClaim: true);

        result.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Tenant")]
    public async Task ClaimReadRoutes_ForAClaimOfAnotherOrganisation_AllAnswer404()
    {
        var (claimId, _) = await _factory.SeedClaimAsync(Guid.NewGuid());
        using var client = CreateClient();

        var result = await RoutesNotAnswering(client, HttpStatusCode.NotFound, claimId, isReadOfClaim: true);

        result.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public void Actions_DoNotUseRoleBasedAuthorizeAttributes()
    {
        var result = ApiActions()
            .Where(action => action.EndpointMetadata.OfType<IAuthorizeData>().Any(data => HasRoles(data)))
            .Select(DescribeAction)
            .ToList();

        result.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public void Actions_AreAuthenticatedExceptTheDevTokenAndTheLocalDownload()
    {
        var result = ApiActions()
            .Where(action => action.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            .Select(action => action.ControllerTypeInfo.Name)
            .Distinct()
            .ToList();

        result.Should().BeEquivalentTo("DevAuthController", "DocumentDownloadController");
    }

    [ApiFact]
    [Trait("Rule", "S:API-Auth")]
    public async Task FallbackPolicy_RequiresAnAuthenticatedUser()
    {
        var provider = _factory.Services.GetRequiredService<IAuthorizationPolicyProvider>();

        var result = await provider.GetFallbackPolicyAsync();

        result!.Requirements.OfType<DenyAnonymousAuthorizationRequirement>().Should().ContainSingle();
    }

    [ApiFact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public void IdempotencyFilter_IsGlobal_SoEveryWriteActionIsCovered()
    {
        var options = _factory.Services.GetRequiredService<IOptions<MvcOptions>>().Value;

        var result = options.Filters.OfType<TypeFilterAttribute>().Select(filter => filter.ImplementationType).ToList();

        result.Should().Contain(typeof(IdempotencyFilter));
    }

    [ApiFact]
    public void BaseControllerActions_DeclareExactlyOneSuccessStatus()
    {
        var result = BaseControllerActions()
            .Where(action => SuccessResponses(action).Count != 1)
            .Select(DescribeAction)
            .ToList();

        result.Should().BeEmpty();
    }

    [ApiFact]
    public void WriteActions_ReturnTheCommandResponseAndReadActionsDoNot()
    {
        var result = BaseControllerActions()
            .Where(action => IsWrite(action) != ReturnsCommandResponse(action))
            .Select(DescribeAction)
            .ToList();

        result.Should().BeEmpty();
    }

    [ApiFact]
    [Trait("Rule", "S:DATA-Naming")]
    public void Routes_AreKebabCasePluralNounsExceptTheListedWords()
    {
        var result = ApiActions()
            .SelectMany(action => LiteralSegments(action.AttributeRouteInfo?.Template ?? string.Empty))
            .Where(segment => !KebabCase().IsMatch(segment) || !IsPluralOrListed(segment))
            .Distinct()
            .ToList();

        result.Should().BeEmpty();
    }

    private static string UrlFor(ControllerActionDescriptor action, Guid claimId)
    {
        string template = action.AttributeRouteInfo?.Template ?? string.Empty;
        string url = "/" + PlaceholderPattern().Replace(template, match => IdFor(match.Value, claimId));

        return action.ActionName == nameof(ClaimsController.CheckStatus) ? url + "?targetStatus=Open" : url;
    }

    private static string IdFor(string placeholder, Guid claimId)
    {
        bool isClaimId = placeholder.StartsWith("{claimId", StringComparison.Ordinal);

        return (isClaimId ? claimId : Guid.NewGuid()).ToString();
    }

    private static bool IsReadOfClaim(ControllerActionDescriptor action)
    {
        string template = action.AttributeRouteInfo?.Template ?? string.Empty;

        return HttpMethodOf(action) == "GET" && template.Contains("{claimId", StringComparison.Ordinal);
    }

    private static string HttpMethodOf(ControllerActionDescriptor action)
    {
        var constraints = action.ActionConstraints?.OfType<HttpMethodActionConstraint>() ?? [];

        return constraints.SelectMany(item => item.HttpMethods).First();
    }

    [GeneratedRegex(@"\{[^}]+\}")]
    private static partial Regex PlaceholderPattern();

    private async Task<List<string>> RoutesNotAnswering(
        HttpClient client,
        HttpStatusCode expected,
        Guid claimId,
        bool isReadOfClaim = false)
    {
        var actions = ApiActions()
            .Where(action => !action.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            .Where(action => !isReadOfClaim || IsReadOfClaim(action));
        var offenders = new List<string>();

        foreach (var action in actions)
        {
            var request = new HttpRequestMessage(new HttpMethod(HttpMethodOf(action)), UrlFor(action, claimId));
            var response = await client.SendAsync(request);

            if (response.StatusCode != expected)
            {
                offenders.Add($"{request.Method} {request.RequestUri} {(int)response.StatusCode}");
            }
        }

        return offenders;
    }

    private static List<ProducesResponseTypeAttribute> SuccessResponses(ControllerActionDescriptor action)
    {
        return action.MethodInfo
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
            .Cast<ProducesResponseTypeAttribute>()
            .Where(response => response.StatusCode is >= 200 and < 300)
            .ToList();
    }

    private static bool HasRoles(IAuthorizeData data)
    {
        return !string.IsNullOrEmpty(data.Roles);
    }

    private static bool IsWrite(ControllerActionDescriptor action)
    {
        var constraints = action.ActionConstraints?.OfType<HttpMethodActionConstraint>() ?? [];
        var methods = constraints.SelectMany(item => item.HttpMethods);

        return methods.Any(method => _writeMethods.Contains(method));
    }

    private static bool ReturnsCommandResponse(ControllerActionDescriptor action)
    {
        var type = SuccessResponses(action).FirstOrDefault()?.Type;

        bool isGeneric = type is { IsGenericType: true };

        return isGeneric && type!.GetGenericTypeDefinition() == typeof(ApiControllerBase.CommandResponse<>);
    }

    private static IEnumerable<string> LiteralSegments(string template)
    {
        return template.Split('/').Where(segment => segment.Length > 0 && !segment.StartsWith('{'));
    }

    private static bool IsPluralOrListed(string segment)
    {
        return segment.EndsWith('s') || _singularSegments.Contains(segment);
    }

    private static string DescribeAction(ControllerActionDescriptor action)
    {
        return $"{action.ControllerTypeInfo.Name}.{action.ActionName}";
    }

    [GeneratedRegex("^[a-z]+(-[a-z]+)*$")]
    private static partial Regex KebabCase();

    private IEnumerable<ControllerActionDescriptor> ApiActions()
    {
        var provider = _factory.Services.GetRequiredService<IActionDescriptorCollectionProvider>();

        return provider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(action => action.ControllerTypeInfo.Assembly == typeof(ApiControllerBase).Assembly);
    }

    private IEnumerable<ControllerActionDescriptor> BaseControllerActions()
    {
        return ApiActions().Where(action => action.ControllerTypeInfo.IsSubclassOf(typeof(ApiControllerBase)));
    }
}
