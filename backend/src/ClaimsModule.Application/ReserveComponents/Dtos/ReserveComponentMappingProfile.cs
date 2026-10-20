using AutoMapper;
using ClaimsModule.Domain.ReserveComponents;

namespace ClaimsModule.Application.ReserveComponents.Dtos;

public sealed class ReserveComponentMappingProfile : Profile
{
    public ReserveComponentMappingProfile()
    {
        CreateMap<ReserveComponent, ReserveComponentDto>()
            .ForMember(dto => dto.ComponentType, options => options.MapFrom(component => component.ComponentType.Name))
            .ForMember(dto => dto.State, options => options.MapFrom(component => component.State.Name));
    }
}
