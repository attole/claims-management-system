using ClaimsModule.API.Pipeline.Health;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ClaimsModule.API.Tests.Pipeline.Health;

public sealed class DatabaseHealthCheckTests
{
    private const string UnreachableConnection = "Server=127.0.0.1,1;Connect Timeout=1;Encrypt=False";

    [Fact]
    public async Task CheckHealthAsync_DatabaseUnreachable_ReturnsUnhealthy()
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>().UseSqlServer(UnreachableConnection).Options;
        using var context = new ClaimsDbContext(options, new Mock<ICurrentUser>().Object);
        var check = new DatabaseHealthCheck(context);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }
}
