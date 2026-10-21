using ClaimsModule.API.Identity;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.DevAuth.Configuration;

internal sealed class DevAuthOptionsValidator : IValidateOptions<DevAuthOptions>
{
    public const int MaxTokenLifetimeMinutes = 1440;

    public ValidateOptionsResult Validate(string? name, DevAuthOptions options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        var failures = ListFailures(options);

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static List<string> ListFailures(DevAuthOptions options)
    {
        var failures = new List<string>();

        failures.AddRange(options.OrganisationId == Guid.Empty ? ["DevAuth:OrganisationId is required."] : []);
        failures.AddRange(options.Users.Count == 0 ? ["DevAuth:Users needs at least one user."] : []);
        bool isLifetimeValid = options.TokenLifetimeMinutes is >= 1 and <= MaxTokenLifetimeMinutes;
        var lifetimeFailure = $"DevAuth:TokenLifetimeMinutes must be 1 to {MaxTokenLifetimeMinutes}.";
        failures.AddRange(isLifetimeValid ? [] : [lifetimeFailure]);
        var invalidUsers = options.Users.Where(user => !IsValid(user));
        failures.AddRange(invalidUsers.Select(user => $"DevAuth user '{user.Name}' is invalid."));

        return failures;
    }

    private static bool IsValid(DevUserOptions user)
    {
        return user.Id != Guid.Empty && user.Name.Length > 0 && RoleNames.IsKnown(user.Role, out _);
    }
}
