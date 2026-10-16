using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Domain.Tests.Claims;

public sealed class ClaimNumberTests
{
    [Fact]
    [Trait("Rule", "S:BR-C-04")]
    [Trait("Rule", "A:BR-C-04")]
    public void From_KeepsAWellFormedNumber()
    {
        var result = ClaimNumber.From("CLM-2026-0000001");

        result.Value.Should().Be("CLM-2026-0000001");
    }

    [Theory]
    [Trait("Rule", "S:BR-C-04")]
    [Trait("Rule", "A:BR-C-04")]
    [InlineData(2026, 1, "CLM-2026-0000001")]
    [InlineData(2026, 1234567, "CLM-2026-1234567")]
    [InlineData(999, 9999999, "CLM-0999-9999999")]
    public void Create_FormatsTheYearAndThePaddedSequence(int year, int sequence, string expected)
    {
        var result = ClaimNumber.Create(year, sequence);

        result.Value.Should().Be(expected);
    }

    [Theory]
    [Trait("Rule", "S:BR-C-04")]
    [Trait("Rule", "A:BR-C-04")]
    [InlineData(2026, 10000000)]
    [InlineData(2026, -1)]
    [InlineData(10000, 1)]
    [InlineData(-2026, 1)]
    public void Create_RejectsAYearOrASequenceThatDoesNotFitTheFormat(int year, int sequence)
    {
        Func<ClaimNumber> act = () => ClaimNumber.Create(year, sequence);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [Trait("Rule", "S:BR-C-04")]
    [Trait("Rule", "A:BR-C-04")]
    [InlineData("")]
    [InlineData(" CLM-2026-0000001")]
    [InlineData("CLM-2026-0000001\n")]
    [InlineData("clm-2026-0000001")]
    [InlineData("CLM-26-0000001")]
    [InlineData("CLM-2026-000001")]
    [InlineData("CLM-2026-00000012")]
    [InlineData("CLM-2026-000000A")]
    [InlineData("CLM-\u0662\u0660\u0662\u0666-0000001")]
    public void From_RejectsAMalformedNumber(string value)
    {
        Func<ClaimNumber> act = () => ClaimNumber.From(value);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Numbers_WithTheSameValueAreEqual()
    {
        ClaimNumber.From("CLM-2026-0000001").Should().Be(ClaimNumber.From("CLM-2026-0000001"));
    }
}
