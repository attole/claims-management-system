using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Core.Rules;

internal interface IRule<in TSubject> : ICheck<TSubject, DomainFinding>;
