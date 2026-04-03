using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Ticketing.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class TicketUpdatedDomainEventHandler
        : IDomainEventHandler<TicketUpdatedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public TicketUpdatedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(TicketUpdatedDomainEvent domainEvent, CancellationToken ct = default)
        {
            foreach (var change in domainEvent.Changes)
            {
                await _notify.NotifyTicketActionAsync(domainEvent.TicketId, change.Description, null, ct);
            }
        }
    }
}