using AutoMapper;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Application.Claims.Dtos;

public sealed class ClaimMappingProfile : Profile
{
    public ClaimMappingProfile()
    {
        CreateMap<Claim, ClaimDto>()
            .ForMember(dto => dto.ClaimNumber, options => options.MapFrom(claim => claim.ClaimNumber.Value))
            .ForMember(dto => dto.State, options => options.MapFrom(claim => claim.State.Name));
    }
}
