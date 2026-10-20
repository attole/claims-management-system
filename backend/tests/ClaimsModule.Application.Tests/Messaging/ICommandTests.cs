using System.Text.RegularExpressions;
using ClaimsModule.Application.Messaging;
using FluentValidation;

namespace ClaimsModule.Application.Tests.Messaging;

public sealed class ICommandTests
{
    private static readonly string[] _verbs =
    [
        "Acknowledge",
        "Add",
        "Approve",
        "Assign",
        "Change",
        "Create",
        "Detect",
        "Link",
        "Mark",
        "Purge",
        "Reject",
        "Remove",
        "Resend",
        "Retract",
        "Retry",
        "Set",
        "Submit",
        "Sweep",
        "Update",
        "Upload",
        "Waive",
    ];

    [Fact]
    [Trait("Rule", "A:VAL-AllCommands")]
    public void Commands_AllHaveAValidator()
    {
        var assembly = typeof(ApplicationServiceExtensions).Assembly;
        var commands = FindCommands();
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

    [Fact]
    [Trait("Rule", "S:DATA-Naming")]
    public void Commands_AreNamedVerbNounCommand()
    {
        var commands = FindCommands();
        var pattern = "^(" + string.Join('|', _verbs) + ")[A-Z][A-Za-z0-9]*Command$";

        var result = commands
            .Where(command => !Regex.IsMatch(command.Name, pattern))
            .ToList();

        commands.Should().NotBeEmpty();
        result.Should().BeEmpty();
    }

    private static List<Type> FindCommands()
    {
        return typeof(ApplicationServiceExtensions).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(IsCommand))
            .ToList();
    }

    private static bool IsCommand(Type contract)
    {
        bool isGeneric = contract.IsGenericType;

        return isGeneric && contract.GetGenericTypeDefinition().Equals(typeof(ICommand<>));
    }
}
