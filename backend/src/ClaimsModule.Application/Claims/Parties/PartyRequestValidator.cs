using ClaimsModule.Domain.Claims.Parties;
using FluentValidation;

namespace ClaimsModule.Application.Claims.Parties;

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
