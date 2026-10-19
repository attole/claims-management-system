using ClaimsModule.Application.Messaging;
using FluentValidation;

namespace ClaimsModule.Application.Tests.Messaging;

public sealed class ICommandTests
{
    [Fact]
    [Trait("Rule", "A:VAL-AllCommands")]
    public void Commands_AllHaveAValidator()
    {
        var assembly = typeof(ApplicationServiceExtensions).Assembly;
        var commands = assembly.GetTypes()
            .Where(type => !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(contract => IsCommand(contract)))
            .ToList();
        var validated = assembly.GetTypes()
            .SelectMany(type => type.GetInterfaces())
            .Where(contract => contract.IsGenericType)
            .Where(contract => contract.GetGenericTypeDefinition().Equals(typeof(IValidator<>)))
            .Select(contract => contract.GenericTypeArguments[0])
            .ToList();

        var result = commands
            .Where(command => !validated.Contains(command))
            .ToList();

        commands.Should().NotBeEmpty();
        result.Should().BeEmpty();
    }

    private static bool IsCommand(Type contract)
    {
        bool isGeneric = contract.IsGenericType;

        return isGeneric && contract.GetGenericTypeDefinition().Equals(typeof(ICommand<>));
    }
}
