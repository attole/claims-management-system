using ClaimsModule.Application.Audit.Contracts;

namespace ClaimsModule.Application.Tests.Audit.Contracts;

public sealed class IAuditLogServiceTests
{
    private const string HandlersNamespace = "ClaimsModule.Application.Audit.Handlers";

    [Fact]
    [Trait("Rule", "S:BR-A-01")]
    public void Service_OnlyAppendsEntries()
    {
        var result = typeof(IAuditLogService)
            .GetMethods()
            .Select(method => method.Name)
            .ToList();

        result.Should().Equal("WriteAsync");
    }

    [Fact]
    [Trait("Rule", "S:AUD-Service")]
    public void Handlers_ReachTheAuditLogOnlyThroughTheService()
    {
        var handlers = typeof(AuditEntry).Assembly
            .GetTypes()
            .Where(type => type.Namespace == HandlersNamespace)
            .Where(type => !type.IsAbstract)
            .Where(type => type.GetConstructors().Length > 0)
            .ToList();

        var result = handlers
            .SelectMany(type => type.GetConstructors().Single().GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Distinct()
            .ToList();

        handlers.Should().NotBeEmpty();
        result.Should().Equal(typeof(IAuditLogService));
    }
}
