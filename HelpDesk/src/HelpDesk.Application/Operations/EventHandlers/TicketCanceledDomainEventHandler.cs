using HelpDesk.Application.Operations.Ports;
using HelpDesk.Domain.Ticketing.Events;
using HelpDesk.Application.Shared.Abstractions;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class TicketCanceledDomainEventHandler
        : IDomainEventHandler<TicketCanceledDomainEvent>
    {
        private readonly INotificationPort _notify;

        public TicketCanceledDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(TicketCanceledDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg = $"Chamado cancelado por {domainEvent.ActorUserName}.";
            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, null, ct);
        }
    }
}