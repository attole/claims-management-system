using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Persistence.Tests.Support.CauseOfLoss;

internal static class TestCauseCodes
{
    public static CauseOfLossCode Create(Guid organisationId, string code)
    {
        return Create(organisationId, code, PerilCategory.Weather, true);
    }

    public static CauseOfLossCode Create(Guid organisationId, string code, PerilCategory perilCategory, bool isActive)
    {
        return new CauseOfLossCode(Ids.Next(), organisationId, code, $"{code} name", perilCategory, isActive);
    }
}
