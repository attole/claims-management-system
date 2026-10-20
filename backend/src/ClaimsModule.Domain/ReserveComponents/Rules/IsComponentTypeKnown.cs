using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.Rules;

internal sealed class IsComponentTypeKnown(ReserveComponentTypeKind kind) : IRule<object>
{
    public const string Code = "S:V-ReserveComponent-Type";

    private const string Message = "Invalid reserve component type.";
    private const string Field = "ComponentType";

    public CheckResult Check(object subject)
    {
        bool isKnown = Enum.IsDefined(kind);

        return isKnown
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
