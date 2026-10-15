using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Core.Rules;

internal sealed class RuleSet<TSubject, TInput> : CheckSet<TSubject, TInput, DomainFinding>;
