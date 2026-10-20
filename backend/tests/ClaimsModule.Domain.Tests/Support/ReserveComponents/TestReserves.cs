using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.Inputs;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Support.ReserveComponents;

public static class TestReserves
{
    private const decimal AutoApprovalLimit = 10_000m;

    public static readonly Guid ClaimId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Submitter = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid Approver = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static ClaimReference Claim(ClaimState state)
    {
        return new ClaimReference(ClaimId, TestClaims.Organisation, state);
    }

    public static ReserveComponent Component()
    {
        var input = new OpenReserveComponentInput(
            Claim(new ClaimOpenState()),
            ReserveComponentTypeKind.Indemnity,
            string.Empty);

        return ReserveComponent.Open(input).Value;
    }

    public static SubmitHistoryInput SubmitInput(decimal amount)
    {
        return new SubmitHistoryInput(
            ReserveTransactionType.Add,
            amount,
            "Initial estimate",
            Submitter,
            new ClaimOpenState(),
            true,
            0m,
            TestClaims.Start);
    }

    internal static ApproveRowInput ApproveInput(UserRole actorRole, Guid actorId)
    {
        return new ApproveRowInput(0m, 0m, false, actorRole, actorId, TestClaims.Later);
    }

    internal static AutoApproveRowInput AutoApproveInput()
    {
        return new AutoApproveRowInput(0m, 0m, Submitter, TestClaims.Later);
    }

    public static ReserveHistory Submit(decimal amount)
    {
        return Submit(SubmitInput(amount));
    }

    public static ReserveHistory Submit(SubmitHistoryInput input)
    {
        return Submit(Component(), input);
    }

    public static ReserveHistory Submit(ReserveComponent component, SubmitHistoryInput input)
    {
        var history = SubmitHistoryOperation.Run(component, input).Value;
        history.PopEvents();
        component.PopEvents();

        return history;
    }

    public static ReserveHistory SubmitRejected()
    {
        var history = Submit(50_000m);
        var input = new RejectRowInput(TestClaims.Supervisor, Approver, TestClaims.Later)
        {
            Reason = "Not supported by the report",
        };
        RejectHistoryTransition.Run(history, input);
        history.PopEvents();

        return history;
    }

    public static ReserveHistory SubmitAutoApproved(decimal amount)
    {
        var history = Submit(amount);
        var input = AutoApproveInput();
        AutoApproveHistoryTransition.Run(history, input);
        history.PopEvents();

        return history;
    }

    public static ReserveComponent ComponentWithApprovedBalance(decimal balance)
    {
        var component = Component();
        decimal remaining = balance;

        while (remaining > 0m)
        {
            decimal amount = Math.Min(remaining, AutoApprovalLimit);
            component.SubmitHistory(SubmitInput(amount));
            remaining -= amount;
        }

        component.PopEvents();

        return component;
    }
}
