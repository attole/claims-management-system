using ClaimsModule.Application.Claims.StateChanges;
using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Application.Tests.Support.Claims;

public static class ClaimStatusContexts
{
    public static ClaimStatusContext Create(ChangeClaimStatusCommand command)
    {
        var facts = new ClaimReserveFacts(false, false, false, false, 0m);

        return Create(command, facts);
    }

    public static ClaimStatusContext Create(ChangeClaimStatusCommand command, ClaimReserveFacts facts)
    {
        return new ClaimStatusContext(
            command,
            ApplicationClaims.LinkedPeriod(),
            new CauseCodes(["WATER"]),
            facts,
            new UserManagerRole());
    }
}
