using AutoMapper;
using ClaimsModule.Domain.ClaimDocuments;

namespace ClaimsModule.Application.ClaimDocuments.Dtos;

public sealed class ClaimDocumentMappingProfile : Profile
{
    public ClaimDocumentMappingProfile()
    {
        CreateMap<ClaimDocument, ClaimDocumentDto>()
            .ForMember(dto => dto.DocumentType, options => options.MapFrom(document => document.Details.DocumentType))
            .ForMember(dto => dto.DocumentName, options => options.MapFrom(document => document.Details.DocumentName))
            .ForMember(dto => dto.ContentType, options => options.MapFrom(document => document.Details.ContentType))
            .ForMember(dto => dto.FileSizeBytes, options => options.MapFrom(document => document.Details.FileSizeBytes))
            .ForMember(dto => dto.Notes, options => options.MapFrom(document => document.Details.Notes));
    }
}
