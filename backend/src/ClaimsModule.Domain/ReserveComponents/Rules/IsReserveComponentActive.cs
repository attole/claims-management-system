using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.Rules;

internal sealed class IsReserveComponentActive : IRule<ReserveComponent>
{
    public const string Code = "RESERVE_COMPONENT_NOT_ACTIVE";

    private const string Message = "The reserve component is closed.";
    private const string Field = "ReserveComponentId";

    public CheckResult Check(ReserveComponent component)
    {
        bool isActive = component.State.IsActive;

        return isActive
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
