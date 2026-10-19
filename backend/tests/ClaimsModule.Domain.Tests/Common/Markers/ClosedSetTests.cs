using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Common.Markers;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Tests.Common.Markers;

public sealed class ClosedSetTests
{
    public static TheoryData<Type, Func<int, IMarked>> Sets => new()
    {
        { typeof(UserRoleKind), kind => UserRole.From((UserRoleKind)kind) },
        { typeof(ClaimStateKind), kind => ClaimState.From((ClaimStateKind)kind) },
        { typeof(PartyRoleKind), kind => PartyRole.From((PartyRoleKind)kind) },
        { typeof(PartyTypeKind), kind => PartyType.From((PartyTypeKind)kind) },
        { typeof(IssueStateKind), kind => IssueState.From((IssueStateKind)kind) },
        { typeof(ReserveComponentTypeKind), kind => ReserveComponentType.From((ReserveComponentTypeKind)kind) },
        { typeof(ReserveComponentStateKind), kind => ReserveComponentState.From((ReserveComponentStateKind)kind) },
        { typeof(ReserveApprovalStateKind), kind => ReserveApprovalState.From((ReserveApprovalStateKind)kind) },
        { typeof(ReservePostingStateKind), kind => ReservePostingState.From((ReservePostingStateKind)kind) },
    };

    [Theory]
    [MemberData(nameof(Sets))]
    public void From_ForEveryKind_ReturnsTheVariantOfThatKind(Type kindType, Func<int, IMarked> from)
    {
        var kinds = Enum.GetValues(kindType)
            .Cast<int>()
            .ToList();
        var expected = Enum.GetNames(kindType);

        var result = kinds
            .Select(from)
            .Select(variant => variant.Name)
            .ToList();

        result.Should().Equal(expected);
    }

    [Fact]
    public void From_ForAnUnknownKind_Throws()
    {
        Func<ClaimState> act = () => ClaimState.From((ClaimStateKind)99);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void From_ForTheSameKind_ReturnsTheSharedVariant()
    {
        var first = ClaimState.From(ClaimStateKind.Open);

        var result = ClaimState.From(ClaimStateKind.Open);

        result.Should().BeSameAs(first);
    }
}
