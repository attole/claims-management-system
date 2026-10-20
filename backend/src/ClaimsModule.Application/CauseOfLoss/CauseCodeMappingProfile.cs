using AutoMapper;
using ClaimsModule.Domain.CauseOfLoss;

namespace ClaimsModule.Application.CauseOfLoss;

public sealed class CauseCodeMappingProfile : Profile
{
    public CauseCodeMappingProfile()
    {
        CreateMap<CauseOfLossCode, CauseCodeDto>();
    }
}
