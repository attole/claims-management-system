using AutoMapper;
using ClaimsModule.Domain.Claims.RiskObjects;

namespace ClaimsModule.Application.Claims.RiskObjects;

public sealed class RiskObjectMappingProfile : Profile
{
    public RiskObjectMappingProfile()
    {
        CreateMap<ClaimRiskObject, RiskObjectDto>()
            .ForMember(dto => dto.AssetType, options => options.MapFrom(riskObject => riskObject.Details.AssetType))
            .ForMember(
                dto => dto.AssetDescription,
                options => options.MapFrom(riskObject => riskObject.Details.AssetDescription))
            .ForMember(
                dto => dto.DamageDescription,
                options => options.MapFrom(riskObject => riskObject.Details.DamageDescription))
            .ForMember(dto => dto.IsPrimary, options => options.MapFrom(riskObject => riskObject.Details.IsPrimary))
            .ForMember(
                dto => dto.AssetReference,
                options => options.MapFrom(riskObject => riskObject.Details.AssetReference));
    }
}
