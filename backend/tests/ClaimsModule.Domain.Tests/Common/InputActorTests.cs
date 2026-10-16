using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Common;

public sealed class InputActorTests
{
    [Fact]
    public void Required_WithAnActor_ReturnsIt()
    {
        var result = InputActor.Required(TestClaims.Actor);

        result.Should().Be(TestClaims.Actor);
    }

    [Fact]
    public void Required_WithoutAnActor_Throws()
    {
        Func<Guid> act = () => InputActor.Required(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
