using HelpDesk.Application.Operations.Ports;
using HelpDesk.Domain.Ticketing.Events;
using HelpDesk.Application.Shared.Abstractions;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class TicketCreatedDomainEventHandler
        : IDomainEventHandler<TicketCreatedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public TicketCreatedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(TicketCreatedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg = $"Chamado criado por {domainEvent.RequesterName}.";
            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, ct: ct);
        }
    }
}

