using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Core.Rules;

public interface IRule<in TSubject> : ICheck<TSubject, DomainFinding>;
