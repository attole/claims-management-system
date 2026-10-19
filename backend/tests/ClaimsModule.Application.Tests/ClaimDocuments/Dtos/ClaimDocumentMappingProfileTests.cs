using AutoMapper;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Tests.Support.ClaimDocuments;
using ClaimsModule.Application.Tests.Support.Claims;

namespace ClaimsModule.Application.Tests.ClaimDocuments.Dtos;

public sealed class ClaimDocumentMappingProfileTests
{
    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        Action act = configuration.AssertConfigurationIsValid;

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_Document_FlattensTheDetails()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var document = ApplicationDocuments.Uploaded(ApplicationClaims.Opened());

        var result = mapper.Map<ClaimDocumentDto>(document);

        result.DocumentName.Should().Be("report.pdf");
        result.ContentType.Should().Be("application/pdf");
        result.FileSizeBytes.Should().Be(2048);
        result.IsDeleted.Should().BeFalse();
    }

    private static MapperConfiguration CreateConfiguration()
    {
        return new MapperConfiguration(configuration => configuration.AddProfile<ClaimDocumentMappingProfile>());
    }
}
