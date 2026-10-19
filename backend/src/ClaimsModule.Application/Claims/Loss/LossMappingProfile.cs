using AutoMapper;
using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Application.Claims.Loss;

public sealed class LossMappingProfile : Profile
{
    public LossMappingProfile()
    {
        CreateMap<LossEvent, LossDetailsDto>()
            .ForMember(dto => dto.LossDate, options => options.MapFrom(loss => loss.Details.LossDate.Value))
            .ForMember(dto => dto.Description, options => options.MapFrom(loss => loss.Details.Description))
            .ForMember(dto => dto.CauseOfLossCode, options => options.MapFrom(loss => loss.Details.CauseOfLossCode))
            .ForMember(dto => dto.Location, options => options.MapFrom(loss => loss.Details.Location))
            .ForMember(
                dto => dto.HasEstimatedLossAmount,
                options => options.MapFrom(loss => loss.Details.EstimatedLossAmount.IsSet))
            .ForMember(
                dto => dto.EstimatedLossAmount,
                options => options.MapFrom(loss => loss.Details.EstimatedLossAmount.Value))
            .ForMember(
                dto => dto.PoliceReportNumber,
                options => options.MapFrom(loss => loss.Details.PoliceReportNumber));
    }
}
