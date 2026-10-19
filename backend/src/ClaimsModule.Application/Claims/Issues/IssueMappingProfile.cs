using AutoMapper;
using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Application.Claims.Issues;

public sealed class IssueMappingProfile : Profile
{
    public IssueMappingProfile()
    {
        CreateMap<ClaimValidationIssue, IssueDto>()
            .ForMember(dto => dto.Code, options => options.MapFrom(issue => issue.Finding.Code))
            .ForMember(dto => dto.Message, options => options.MapFrom(issue => issue.Finding.Message))
            .ForMember(dto => dto.Field, options => options.MapFrom(issue => issue.Finding.Field))
            .ForMember(dto => dto.Severity, options => options.MapFrom(issue => issue.Finding.Severity))
            .ForMember(dto => dto.State, options => options.MapFrom(issue => issue.State.Name));
    }
}
