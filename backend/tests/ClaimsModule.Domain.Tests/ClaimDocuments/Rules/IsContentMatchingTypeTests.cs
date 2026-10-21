using System.Text;
using ClaimsModule.Domain.ClaimDocuments.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.ClaimDocuments.Rules;

public sealed class IsContentMatchingTypeTests
{
    private const string WordType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    private const string ExcelType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [Theory]
    [Trait("Rule", IsContentMatchingType.Code)]
    [InlineData("application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31 })]
    [InlineData("APPLICATION/PDF", new byte[] { 0x25, 0x50, 0x44, 0x46 })]
    [InlineData("image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 })]
    [InlineData("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 })]
    [InlineData(WordType, new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14 })]
    [InlineData(ExcelType, new byte[] { 0x50, 0x4B, 0x03, 0x04 })]
    [InlineData("text/plain", new byte[] { 0x48, 0x69 })]
    [InlineData("text/csv", new byte[] { })]
    [InlineData("application/x-msdownload", new byte[] { 0x4D, 0x5A })]
    public void Check_ForContentThatMatchesItsType_Passes(string contentType, byte[] header)
    {
        var rule = new IsContentMatchingType(contentType, header);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Theory]
    [Trait("Rule", IsContentMatchingType.Code)]
    [InlineData("application/pdf", new byte[] { 0x4D, 0x5A, 0x90, 0x00 })]
    [InlineData("application/pdf", new byte[] { })]
    [InlineData("image/png", new byte[] { 0xFF, 0xD8, 0xFF })]
    [InlineData("image/jpeg", new byte[] { 0xFF, 0xD8 })]
    [InlineData(WordType, new byte[] { 0x25, 0x50, 0x44, 0x46 })]
    [InlineData("text/plain", new byte[] { 0x4D, 0x5A, 0x00, 0x03 })]
    public void Check_ForContentThatDoesNotMatchItsType_ReturnsTheFinding(string contentType, byte[] header)
    {
        var rule = new IsContentMatchingType(contentType, header);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsContentMatchingType.Code);
    }

    [Fact]
    [Trait("Rule", IsContentMatchingType.Code)]
    public void Check_ForCsvText_Passes()
    {
        var rule = new IsContentMatchingType("text/csv", Encoding.UTF8.GetBytes("claim,amount\nCLM-1,100\n"));

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }
}
