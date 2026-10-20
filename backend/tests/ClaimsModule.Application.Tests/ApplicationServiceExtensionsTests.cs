using System.Text.RegularExpressions;
using ClaimsModule.Application.Tests.Support.Assemblies;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Application.Tests;

public sealed class ApplicationServiceExtensionsTests
{
    private const string GuidPattern = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

    [Fact]
    [Trait("Rule", "S:DATA-NoHardGuids")]
    public void Assemblies_HaveNoHardCodedGuidLiteral()
    {
        var assemblies = new[] { typeof(ApplicationServiceExtensions).Assembly, typeof(Ids).Assembly };

        var result = assemblies
            .SelectMany(AssemblyStrings.Literals)
            .Where(literal => Regex.IsMatch(literal, GuidPattern))
            .ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:DATA-NoHardGuids")]
    public void Scan_FindsAGuidLiteralInAnAssemblyThatHasOne()
    {
        var assembly = typeof(ApplicationServiceExtensionsTests).Assembly;

        var result = AssemblyStrings.Literals(assembly)
            .Where(literal => Regex.IsMatch(literal, GuidPattern))
            .ToList();

        result.Should().NotBeEmpty();
    }
}
