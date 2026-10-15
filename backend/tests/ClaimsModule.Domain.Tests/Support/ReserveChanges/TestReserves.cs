using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Operations;
using ClaimsModule.Domain.ReserveChanges.Root.Transitions;
using ClaimsModule.Domain.Reserves.Root;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Support.ReserveChanges;

public static class TestReserves
{
    public static readonly Guid ReserveId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ClaimId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid Submitter = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid Approver = Guid.Parse("66666666-6666-6666-6666-666666666666");

    public static ReserveSnapshot Snapshot()
    {
        return new ReserveSnapshot(true, 0m, false, new ReserveIndemnityComponentType(), 0m, 1);
    }

    public static SubmitReserveInput SubmitInput(decimal amount, ReserveSnapshot snapshot)
    {
        return new SubmitReserveInput(
            TestClaims.Organisation,
            ReserveId,
            ClaimId,
            ReserveTransactionType.Add,
            amount,
            "Initial estimate",
            Submitter,
            snapshot,
            TestClaims.Start);
    }

    public static ReserveChange Submit(decimal amount)
    {
        return Submit(amount, Snapshot());
    }

    public static ReserveChange Submit(decimal amount, ReserveSnapshot snapshot)
    {
        var input = SubmitInput(amount, snapshot);
        var reserve = new SubmitReserveOperation().Execute(input).Value;
        reserve.PopEvents();

        return reserve;
    }

    public static ReserveChange SubmitAutoApproved(decimal amount)
    {
        var reserve = Submit(amount);
        var input = new AutoApproveReserveInput(TestClaims.Handler, Submitter, TestClaims.Later, Snapshot());
        new AutoApproveReserveTransition().Execute(reserve, input);
        reserve.PopEvents();

        return reserve;
    }
}
