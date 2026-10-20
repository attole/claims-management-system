using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Persistence.Tests.Support.CauseOfLoss;

internal static class TestCauseCodes
{
    public static CauseOfLossCode Create(Guid organisationId, string code)
    {
        return new CauseOfLossCode(Ids.Next(), organisationId, code, "Hail damage", PerilCategory.Weather, true);
    }
}
