using ClaimsModule.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClaimsModule.Persistence.Tests.Support;

internal static class SqlServerModel
{
    public static IModel Create()
    {
        var user = new Mock<ICurrentUser>();
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseSqlServer("Server=.;Database=ClaimsModule")
            .Options;
        using var context = new ClaimsDbContext(options, user.Object);

        return context.GetService<IDesignTimeModel>().Model;
    }
}
