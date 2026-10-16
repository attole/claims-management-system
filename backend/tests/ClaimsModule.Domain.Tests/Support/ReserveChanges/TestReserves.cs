using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.Roles;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Support.ReserveChanges;

public static class TestReserves
{
    public static readonly Guid ReserveId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ClaimId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Submitter = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid Approver = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static ReserveReference Reference()
    {
        return new ReserveReference(ReserveId, ClaimId, TestClaims.Organisation, new ReserveIndemnityComponentType());
    }

    public static SubmitReserveInput SubmitInput(decimal amount, int changeSequence = 1)
    {
        return new SubmitReserveInput(
            Reference(),
            ReserveTransactionType.Add,
            amount,
            "Initial estimate",
            Submitter,
            true,
            true,
            0m,
            0m,
            changeSequence,
            TestClaims.Start);
    }

    public static ApproveReserveInput ApproveInput(UserRole actorRole, Guid actorId)
    {
        return new ApproveReserveInput(0m, 0m, false, actorRole, actorId, TestClaims.Later);
    }

    public static AutoApproveReserveInput AutoApproveInput()
    {
        return new AutoApproveReserveInput(0m, 0m, Submitter, TestClaims.Later);
    }

    public static ReserveChange Submit(decimal amount)
    {
        return Submit(SubmitInput(amount));
    }

    public static ReserveChange Submit(SubmitReserveInput input)
    {
        var change = SubmitReserveOperation.Run(input).Value;
        change.PopEvents();

        return change;
    }

    public static ReserveChange SubmitRejected()
    {
        var change = Submit(50_000m);
        var input = new RejectReserveInput(TestClaims.Supervisor, Approver, TestClaims.Later)
        {
            Reason = "Not supported by the report",
        };
        RejectReserveTransition.Run(change, input);
        change.PopEvents();

        return change;
    }

    public static ReserveChange SubmitAutoApproved(decimal amount)
    {
        var change = Submit(amount);
        var input = AutoApproveInput();
        AutoApproveReserveTransition.Run(change, input);
        change.PopEvents();

        return change;
    }
}
