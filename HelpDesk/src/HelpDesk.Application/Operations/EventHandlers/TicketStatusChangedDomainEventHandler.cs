using HelpDesk.Application.Operations.Ports;
using HelpDesk.Domain.Ticketing.Events;
using HelpDesk.Application.Shared.Abstractions;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class TicketStatusChangedDomainEventHandler
        : IDomainEventHandler<TicketStatusChangedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public TicketStatusChangedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(TicketStatusChangedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg = $"Status do Chamado atualizado de: '{domainEvent.PreviousStatus}' para: '{domainEvent.NewStatus}' por {domainEvent.PerformedByUserName}.";
            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, null, ct);
        }
    }
}