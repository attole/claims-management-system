using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Domain.Claims;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.ClaimNumbers;

internal sealed class ClaimNumberGenerator(ClaimsDbContext context) : IClaimNumberGenerator
{
    private const int MaxSequence = 9_999_999;

    public async Task<ClaimNumber> NextAsync(int year, CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction is null)
        {
            await context.Database.BeginTransactionAsync(cancellationToken);
        }

        var organisationId = context.CurrentOrganisationId;
        var values = await context.Database
            .SqlQuery<int>($"""
                DECLARE @next int;
                UPDATE ClaimNumberCounters WITH (UPDLOCK, HOLDLOCK)
                SET @next = LastValue = LastValue + 1
                WHERE OrganisationId = {organisationId} AND [Year] = {year};
                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO ClaimNumberCounters (OrganisationId, [Year], LastValue)
                    VALUES ({organisationId}, {year}, 1);
                    SET @next = 1;
                END
                SELECT @next AS Value;
                """)
            .ToListAsync(cancellationToken);
        var sequence = values.Single();

        if (sequence > MaxSequence)
        {
            throw new ConcurrencyConflictException(
                "The claim numbers of the year are used up.",
                new InvalidOperationException($"Sequence {sequence} is past {MaxSequence}."));
        }

        return ClaimNumber.Create(year, sequence);
    }
}
