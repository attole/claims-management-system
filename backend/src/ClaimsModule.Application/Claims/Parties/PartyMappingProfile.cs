using AutoMapper;
using ClaimsModule.Domain.Claims.Parties;

namespace ClaimsModule.Application.Claims.Parties;

public sealed class PartyMappingProfile : Profile
{
    public PartyMappingProfile()
    {
        CreateMap<ClaimParty, PartyDto>()
            .ForMember(dto => dto.Role, options => options.MapFrom(party => party.Details.Role.Name))
            .ForMember(dto => dto.Type, options => options.MapFrom(party => party.Details.Type.Name))
            .ForMember(dto => dto.FirstName, options => options.MapFrom(party => party.Details.FirstName))
            .ForMember(dto => dto.LastName, options => options.MapFrom(party => party.Details.LastName))
            .ForMember(dto => dto.CompanyName, options => options.MapFrom(party => party.Details.CompanyName))
            .ForMember(dto => dto.Email, options => options.MapFrom(party => party.Details.Email))
            .ForMember(dto => dto.Phone, options => options.MapFrom(party => party.Details.Phone))
            .ForMember(dto => dto.Notes, options => options.MapFrom(party => party.Details.Notes));
    }
}
