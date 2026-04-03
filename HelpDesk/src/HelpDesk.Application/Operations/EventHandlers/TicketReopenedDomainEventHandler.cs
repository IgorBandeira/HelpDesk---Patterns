using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Ticketing.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class TicketReopenedDomainEventHandler
        : IDomainEventHandler<TicketReopenedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public TicketReopenedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(TicketReopenedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg = $"Chamado reaberto por {domainEvent.ActorUserName}.";
            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, null, ct);
        }
    }
}