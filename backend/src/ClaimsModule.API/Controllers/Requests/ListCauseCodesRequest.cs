using ClaimsModule.Application.CauseOfLoss.Queries;
using ClaimsModule.Domain.CauseOfLoss;

namespace ClaimsModule.API.Controllers.Requests;

public sealed record ListCauseCodesRequest
{
    public PerilCategory? PerilCategory { get; init; }

    public ListCauseCodesQuery ToQuery()
    {
        return new ListCauseCodesQuery { PerilCategory = PerilCategory };
    }
}
