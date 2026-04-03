using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.UnitTests.Shared.Fakes
{
    public sealed class FakeDomainEventDispatcher : IDomainEventDispatcher
    {
        public List<DomainEvent> DispatchedEvents { get; } = new();

        public Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken ct = default)
        {
            DispatchedEvents.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }
}