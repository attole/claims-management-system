using ClaimsModule.Application.Idempotency;

namespace ClaimsModule.Application.Tests.Idempotency;

public sealed class PurgeExpiredIdempotencyKeysCommandTests
{
    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public async Task Handle_ReportsHowManyKeysTheStorePurged()
    {
        var store = new Mock<IIdempotencyStore>();
        store.Setup(mock => mock.PurgeExpiredAsync(It.IsAny<CancellationToken>())).ReturnsAsync(4);
        var handler = new PurgeExpiredIdempotencyKeysCommandHandler(store.Object);

        var result = await handler.Handle(new PurgeExpiredIdempotencyKeysCommand(), CancellationToken.None);

        result.Value.PurgedCount.Should().Be(4);
    }
}
