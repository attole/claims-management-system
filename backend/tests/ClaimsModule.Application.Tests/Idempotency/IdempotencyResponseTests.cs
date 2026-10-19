using ClaimsModule.Application.Idempotency;

namespace ClaimsModule.Application.Tests.Idempotency;

public sealed class IdempotencyResponseTests
{
    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public void IsSet_ForTheEmptyResponse_IsFalse()
    {
        var response = IdempotencyResponse.Empty;

        var result = response.IsSet;

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public void IsSet_ForAStoredResponse_IsTrue()
    {
        var response = new IdempotencyResponse(201, "{}");

        var result = response.IsSet;

        result.Should().BeTrue();
    }
}
