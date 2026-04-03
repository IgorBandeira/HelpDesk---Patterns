using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Application.Shared.Abstractions
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken ct = default);
    }
}