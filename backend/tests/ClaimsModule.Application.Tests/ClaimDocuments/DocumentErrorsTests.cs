using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Application.Results;

namespace ClaimsModule.Application.Tests.ClaimDocuments;

public sealed class DocumentErrorsTests
{
    [Fact]
    public void NotFound_IsANotFoundErrorWithItsCode()
    {
        var error = DocumentErrors.NotFound(Guid.NewGuid());

        error.Kind.Should().Be(ErrorKind.NotFound);
        error.Code.Should().Be(DocumentErrors.NotFoundCode);
    }
}
