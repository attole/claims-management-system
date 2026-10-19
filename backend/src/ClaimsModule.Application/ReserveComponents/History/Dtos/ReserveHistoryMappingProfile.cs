using AutoMapper;
using ClaimsModule.Domain.ReserveComponents.History;

namespace ClaimsModule.Application.ReserveComponents.History.Dtos;

public sealed class ReserveHistoryMappingProfile : Profile
{
    public ReserveHistoryMappingProfile()
    {
        CreateMap<ReserveHistory, ReserveHistoryDto>()
            .ForMember(dto => dto.ClaimId, options => options.MapFrom(history => history.Details.ClaimId))
            .ForMember(
                dto => dto.ComponentType,
                options => options.MapFrom(history => history.Details.ComponentType.Name))
            .ForMember(
                dto => dto.TransactionType,
                options => options.MapFrom(history => history.Details.TransactionType))
            .ForMember(dto => dto.Amount, options => options.MapFrom(history => history.Details.Amount))
            .ForMember(dto => dto.ChangeReason, options => options.MapFrom(history => history.Details.ChangeReason))
            .ForMember(
                dto => dto.ChangeSequence,
                options => options.MapFrom(history => history.Details.ChangeSequence))
            .ForMember(dto => dto.ApprovalStatus, options => options.MapFrom(history => history.ApprovalState.Name))
            .ForMember(dto => dto.PostingStatus, options => options.MapFrom(history => history.PostingState.Name))
            .ForMember(dto => dto.ReviewedBy, options => options.MapFrom(history => history.Review.ReviewedBy))
            .ForMember(dto => dto.ReviewedAt, options => options.MapFrom(history => history.Review.ReviewedAt))
            .ForMember(dto => dto.ReviewReason, options => options.MapFrom(history => history.Review.Reason))
            .ForMember(dto => dto.JobId, options => options.MapFrom(history => history.PostingDetails.JobId))
            .ForMember(
                dto => dto.FailureReason,
                options => options.MapFrom(history => history.PostingDetails.FailureReason));
    }
}
