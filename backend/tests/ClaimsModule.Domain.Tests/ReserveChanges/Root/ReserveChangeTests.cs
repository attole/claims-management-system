using ClaimsModule.Domain.ReserveChanges.Root.Transitions;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root;

public sealed class ReserveChangeTests
{
    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_ForAnAutoApprovedRow_IsTrue()
    {
        var reserve = TestReserves.SubmitAutoApproved(5_000m);

        reserve.IsApproved.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_ForAPendingRow_IsFalse()
    {
        var reserve = TestReserves.Submit(50_000m);

        reserve.IsApproved.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_AfterAnApproval_IsTrue()
    {
        var reserve = TestReserves.Submit(50_000m);
        var input = new ApproveReserveInput(
            TestClaims.Supervisor,
            TestReserves.Approver,
            TestClaims.Later,
            TestReserves.Snapshot());
        new ApproveReserveTransition().Execute(reserve, input);

        reserve.IsApproved.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsRejected_ForAPendingRow_IsFalse()
    {
        var reserve = TestReserves.Submit(50_000m);

        reserve.IsRejected.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsRejected_AfterARejection_IsTrue()
    {
        var reserve = TestReserves.Submit(50_000m);
        var snapshot = TestReserves.Snapshot();
        var input = new RejectReserveInput(TestClaims.Supervisor, TestReserves.Approver, TestClaims.Later, snapshot)
        {
            Reason = "Not supported by the report",
        };
        new RejectReserveTransition().Execute(reserve, input);

        reserve.IsRejected.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void IsApproved_AfterARejection_IsFalse()
    {
        var reserve = TestReserves.Submit(50_000m);
        var snapshot = TestReserves.Snapshot();
        var input = new RejectReserveInput(TestClaims.Supervisor, TestReserves.Approver, TestClaims.Later, snapshot)
        {
            Reason = "Not supported by the report",
        };
        new RejectReserveTransition().Execute(reserve, input);

        reserve.IsApproved.Should().BeFalse();
    }
}
