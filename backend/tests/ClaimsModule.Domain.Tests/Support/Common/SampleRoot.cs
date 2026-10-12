using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Tests.Support.Common;

public sealed class SampleRoot(Guid id) : AggregateRoot(id);
