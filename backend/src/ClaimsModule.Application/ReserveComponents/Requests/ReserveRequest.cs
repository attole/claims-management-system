using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.Inputs;
using FluentValidation;

namespace ClaimsModule.Application.ReserveComponents.Requests;

public sealed record ReserveRequest(ReserveTransactionType TransactionType, decimal Amount, string ChangeReason)
{
    public Guid ComponentId { get; init; } = Guid.Empty;

    public ReserveComponentTypeKind? ComponentType { get; init; }

    public string Notes { get; init; } = string.Empty;

    public bool IsNewComponent => ComponentType.HasValue;

    public bool HasComponentId => ComponentId != Guid.Empty;

    public OpenReserveComponentInput ToOpenInput(ClaimReference claim)
    {
        var componentType = ReserveComponentType.From(ComponentType.GetValueOrDefault());

        return new OpenReserveComponentInput(claim, componentType, Notes ?? string.Empty);
    }

    public SubmitHistoryInput ToSubmitInput(Claim claim, decimal approvedTotal, Guid actorId, DateTimeOffset now)
    {
        return new SubmitHistoryInput(
            TransactionType,
            Amount,
            ChangeReason ?? string.Empty,
            actorId,
            claim.State,
            claim.HasLinkedPolicy,
            approvedTotal,
            now);
    }
}

public sealed class ReserveRequestValidator : AbstractValidator<ReserveRequest>
{
    private const string ExactlyOneComponentMessage = "Give exactly one of a component type or a component id.";

    public ReserveRequestValidator()
    {
        RuleFor(request => request.TransactionType).IsInEnum();
        RuleFor(request => request.ComponentType).IsInEnum().When(request => request.IsNewComponent);
        RuleFor(request => request.ChangeReason).NotNull().MaximumLength(ReserveFieldLengths.ChangeReason);
        RuleFor(request => request.ComponentId)
            .Must((request, _) => request.IsNewComponent != request.HasComponentId)
            .WithMessage(ExactlyOneComponentMessage);
    }
}
