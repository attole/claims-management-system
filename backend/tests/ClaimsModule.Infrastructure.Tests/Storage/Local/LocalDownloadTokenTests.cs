using ClaimsModule.Infrastructure.Storage.Local;
using ClaimsModule.Infrastructure.Tests.Support;

namespace ClaimsModule.Infrastructure.Tests.Storage.Local;

public sealed class LocalDownloadTokenTests
{
    private static readonly TimeSpan _oneHour = TimeSpan.FromHours(1);

    private readonly LocalDownloadToken _issuer = TokenAt(StorageTestData.Now);

    [Fact]
    [Trait("Rule", "S:BR-D-02")]
    public void ReadPath_TokenWithinItsLifetime_ReturnsThePath()
    {
        var token = _issuer.Create(StorageTestData.BlobPath, _oneHour);
        var reader = TokenAt(StorageTestData.Now.AddMinutes(59));

        var result = reader.ReadPath(token);

        result.Should().Be(StorageTestData.BlobPath);
    }

    [Fact]
    [Trait("Rule", "S:BR-D-02")]
    public void ReadPath_TokenAfterItsLifetime_ReturnsEmpty()
    {
        var token = _issuer.Create(StorageTestData.BlobPath, _oneHour);
        var reader = TokenAt(StorageTestData.Now.AddMinutes(61));

        var result = reader.ReadPath(token);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReadPath_PayloadOfAnotherToken_ReturnsEmpty()
    {
        var signed = _issuer.Create(StorageTestData.BlobPath, _oneHour);
        var other = _issuer.Create("claim-documents/org-2/claim-2/other.pdf", _oneHour);
        var tampered = $"{other.Split('.')[0]}.{signed.Split('.')[1]}";

        var result = _issuer.ReadPath(tampered);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReadPath_TokenSignedWithAnotherKey_ReturnsEmpty()
    {
        var foreign = TokenAt(StorageTestData.Now, "another-signing-key");
        var token = foreign.Create(StorageTestData.BlobPath, _oneHour);

        var result = _issuer.ReadPath(token);

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("abc")]
    [InlineData("a.b.c")]
    [InlineData("!!.!!")]
    [InlineData("YQ.YQ")]
    public void ReadPath_MalformedToken_ReturnsEmpty(string token)
    {
        var result = _issuer.ReadPath(token);

        result.Should().BeEmpty();
    }

    private static LocalDownloadToken TokenAt(DateTimeOffset now, string signingKey = StorageTestData.SigningKey)
    {
        return new LocalDownloadToken(StorageTestData.Local("unused", signingKey), StorageTestData.ClockAt(now));
    }
}
