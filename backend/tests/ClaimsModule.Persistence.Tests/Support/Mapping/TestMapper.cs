using AutoMapper;
using ClaimsModule.Application.ClaimDocuments.Dtos;

namespace ClaimsModule.Persistence.Tests.Support.Mapping;

internal static class TestMapper
{
    public static IMapper Create()
    {
        var applicationAssembly = typeof(ClaimDocumentMappingProfile).Assembly;
        var configuration = new MapperConfiguration(options => options.AddMaps(applicationAssembly));

        return configuration.CreateMapper();
    }
}
