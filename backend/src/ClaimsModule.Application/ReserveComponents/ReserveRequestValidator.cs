using FluentValidation;

namespace ClaimsModule.Application.ReserveComponents;

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
