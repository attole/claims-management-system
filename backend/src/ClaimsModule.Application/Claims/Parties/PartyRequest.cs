using ClaimsModule.Domain.Claims.Parties;
using FluentValidation;

namespace ClaimsModule.Application.Claims.Parties;

public sealed record PartyRequest(PartyRoleKind Role, PartyTypeKind Type)
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public PartyDetails ToDetails()
    {
        var role = PartyRole.From(Role);
        var type = PartyType.From(Type);

        return new PartyDetails(role, type)
        {
            FirstName = FirstName ?? string.Empty,
            LastName = LastName ?? string.Empty,
            CompanyName = CompanyName ?? string.Empty,
            Email = Email ?? string.Empty,
            Phone = Phone ?? string.Empty,
            Notes = Notes ?? string.Empty,
        };
    }
}

public sealed class PartyRequestValidator : AbstractValidator<PartyRequest>
{
    public PartyRequestValidator()
    {
        RuleFor(request => request.Role).IsInEnum();
        RuleFor(request => request.Type).IsInEnum();
        RuleFor(request => request.FirstName).NotEmpty().When(IsPersonWithoutName);
        RuleFor(request => request.CompanyName).NotEmpty().When(IsCompanyWithoutName);
        RuleFor(request => request.FirstName).MaximumLength(ClaimFieldLengths.FirstName);
        RuleFor(request => request.LastName).MaximumLength(ClaimFieldLengths.LastName);
        RuleFor(request => request.CompanyName).MaximumLength(ClaimFieldLengths.CompanyName);
        RuleFor(request => request.Email).MaximumLength(ClaimFieldLengths.Email);
        RuleFor(request => request.Email).EmailAddress().When(request => !string.IsNullOrEmpty(request.Email));
        RuleFor(request => request.Phone).MaximumLength(ClaimFieldLengths.Phone);
    }

    private static bool IsPersonWithoutName(PartyRequest request)
    {
        bool isPerson = request.Type == PartyTypeKind.Person;
        bool hasName = !string.IsNullOrWhiteSpace(request.FirstName) || !string.IsNullOrWhiteSpace(request.LastName);

        return isPerson && !hasName;
    }

    private static bool IsCompanyWithoutName(PartyRequest request)
    {
        bool isCompany = request.Type == PartyTypeKind.Company;
        bool hasName = !string.IsNullOrWhiteSpace(request.CompanyName);

        return isCompany && !hasName;
    }
}
