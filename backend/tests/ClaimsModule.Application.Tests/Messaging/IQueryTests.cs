using System.Text.RegularExpressions;
using ClaimsModule.Application.Messaging;

namespace ClaimsModule.Application.Tests.Messaging;

public sealed class IQueryTests
{
    private const string QuerySuffix = "Query";
    private const string ListPrefix = "List";

    [Fact]
    [Trait("Rule", "S:DATA-Naming")]
    public void Queries_AreNamedGetNounOrListNouns()
    {
        var queries = FindQueries();

        var result = queries
            .Where(query => !Regex.IsMatch(query.Name, "^(Get|List)[A-Z][A-Za-z0-9]*Query$"))
            .ToList();

        queries.Should().NotBeEmpty();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", "S:DATA-Naming")]
    public void ListQueries_NameAPluralNoun()
    {
        var lists = FindQueries()
            .Where(query => query.Name.StartsWith(ListPrefix, StringComparison.Ordinal))
            .ToList();

        var result = lists
            .Where(query => !query.Name[..^QuerySuffix.Length].EndsWith('s'))
            .ToList();

        lists.Should().NotBeEmpty();
        result.Should().BeEmpty();
    }

    private static List<Type> FindQueries()
    {
        return typeof(ApplicationServiceExtensions).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(IsQuery))
            .ToList();
    }

    private static bool IsQuery(Type contract)
    {
        bool isGeneric = contract.IsGenericType;

        return isGeneric && contract.GetGenericTypeDefinition().Equals(typeof(IQuery<>));
    }
}
