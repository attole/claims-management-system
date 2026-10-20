using ClaimsModule.Application.Audit.Contracts;

namespace ClaimsModule.Application.Tests.Audit.Contracts;

public sealed class IAuditLogReaderTests
{
    [Fact]
    [Trait("Rule", "S:BR-A-01")]
    public void Reader_OnlyListsEntries()
    {
        var result = typeof(IAuditLogReader)
            .GetMethods()
            .Select(method => method.Name)
            .ToList();

        result.Should().OnlyContain(name => name.StartsWith("List", StringComparison.Ordinal));
    }
}
