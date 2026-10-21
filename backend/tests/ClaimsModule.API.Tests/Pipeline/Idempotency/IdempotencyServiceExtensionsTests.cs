using ClaimsModule.API.Pipeline.Idempotency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.Tests.Pipeline.Idempotency;

public sealed class IdempotencyServiceExtensionsTests
{
    [Fact]
    [Trait("Rule", "S:API-IdempotencyKey")]
    public void AddIdempotency_AddsTheFilterGloballyToEveryAction()
    {
        var services = new ServiceCollection().AddOptions().AddIdempotency();
        using var provider = services.BuildServiceProvider();

        var filters = provider.GetRequiredService<IOptions<MvcOptions>>().Value.Filters;

        var types = filters.OfType<TypeFilterAttribute>().Select(filter => filter.ImplementationType);
        types.Should().Contain(typeof(IdempotencyFilter));
    }
}
