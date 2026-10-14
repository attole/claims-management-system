using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Core.Rules;

public sealed class RuleSet<TSubject, TInput> : CheckSet<TSubject, TInput, DomainFinding>;
