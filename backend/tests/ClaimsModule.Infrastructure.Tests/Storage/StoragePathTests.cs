using ClaimsModule.Infrastructure.Storage;

namespace ClaimsModule.Infrastructure.Tests.Storage;

public sealed class StoragePathTests
{
    [Fact]
    public void From_ContainerAndName_SplitsAtTheFirstSlash()
    {
        var result = StoragePath.From("claim-documents/org/claim/file.pdf");

        result.Should().Be(new StoragePath("claim-documents", "org/claim/file.pdf"));
    }

    [Theory]
    [InlineData("file.pdf")]
    [InlineData("/file.pdf")]
    [InlineData("")]
    public void From_PathWithoutAContainer_Throws(string path)
    {
        Func<StoragePath> act = () => StoragePath.From(path);

        act.Should().Throw<ArgumentException>();
    }
}
