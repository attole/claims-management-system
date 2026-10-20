using ClaimsModule.Application.Abstractions;
using ClaimsModule.Persistence.Interceptors;

namespace ClaimsModule.Persistence.Tests.Interceptors;

public sealed class SaveInterceptorsTests
{
    [Fact]
    public void Create_Interceptors_RunInTheOrderTheStepsDependOn()
    {
        var clock = new Mock<IClock>();
        var user = new Mock<ICurrentUser>();

        var result = SaveInterceptors.Create(clock.Object, user.Object);

        result.Select(interceptor => interceptor.GetType()).Should().Equal(
            typeof(RootTouchInterceptor),
            typeof(AppendOnlyInterceptor),
            typeof(SoftDeleteInterceptor),
            typeof(RowStampInterceptor));
    }
}
