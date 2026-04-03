using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Application.Shared.Abstractions
{
    public interface IDomainEventHandler<in TDomainEvent>
        where TDomainEvent : DomainEvent
    {
        Task HandleAsync(TDomainEvent domainEvent, CancellationToken ct = default);
    }
}