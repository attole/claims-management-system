using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Parties;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims.Parties;
using FluentValidation.TestHelper;

namespace ClaimsModule.Application.Tests.Claims.Parties;

public sealed class PartyRequestValidatorTests
{
    [Fact]
    public void Validate_ForAKnownRoleAndType_HasNoErrors()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant();

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAnUnknownRole_FailsOnTheRole()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with { Role = (PartyRoleKind)99 };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.Role);
    }

    [Fact]
    public void Validate_WithAnUnknownType_FailsOnTheType()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with { Type = (PartyTypeKind)99 };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.Type);
    }

    [Fact]
    public void Validate_WithATooLongFirstName_FailsOnTheField()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with
        {
            FirstName = new string('x', ClaimFieldLengths.FirstName + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.FirstName);
    }

    [Fact]
    public void Validate_WithATooLongLastName_FailsOnTheField()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with
        {
            LastName = new string('x', ClaimFieldLengths.LastName + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.LastName);
    }

    [Fact]
    public void Validate_WithATooLongCompanyName_FailsOnTheField()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with
        {
            CompanyName = new string('x', ClaimFieldLengths.CompanyName + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.CompanyName);
    }

    [Fact]
    public void Validate_WithATooLongEmail_FailsOnTheField()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with
        {
            Email = new string('x', ClaimFieldLengths.Email + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.Email);
    }

    [Fact]
    public void Validate_WithATooLongPhone_FailsOnTheField()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with
        {
            Phone = new string('x', ClaimFieldLengths.Phone + 1),
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.Phone);
    }

    [Fact]
    public void Validate_ForAPersonWithOnlyALastName_HasNoErrors()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with { FirstName = string.Empty };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ForAPersonWithoutAName_FailsOnTheFirstName()
    {
        var validator = new PartyRequestValidator();
        var request = ApplicationRequests.Claimant() with
        {
            FirstName = string.Empty,
            LastName = string.Empty,
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.FirstName);
    }

    [Fact]
    public void Validate_ForACompanyWithoutAName_FailsOnTheCompanyName()
    {
        var validator = new PartyRequestValidator();
        var request = new PartyRequest(PartyRoleKind.Insured, PartyTypeKind.Company);

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(party => party.CompanyName);
    }

    [Fact]
    public void Validate_ForACompanyWithAName_HasNoErrors()
    {
        var validator = new PartyRequestValidator();
        var request = new PartyRequest(PartyRoleKind.Insured, PartyTypeKind.Company) { CompanyName = "Stone Ltd" };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
