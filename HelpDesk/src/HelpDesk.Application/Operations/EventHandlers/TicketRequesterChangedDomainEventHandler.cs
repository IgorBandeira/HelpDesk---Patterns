using HelpDesk.Application.Operations.Ports;
using HelpDesk.Domain.Ticketing.Events;
using HelpDesk.Application.Shared.Abstractions;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class TicketRequesterChangedDomainEventHandler
        : IDomainEventHandler<TicketRequesterChangedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public TicketRequesterChangedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(TicketRequesterChangedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg = $"{domainEvent.PerformedByUserName} mudou requester para {domainEvent.RequesterName}.";
            await _notify.NotifyTicketActionAsync(
                domainEvent.TicketId,
                msg,
                extraEmail: domainEvent.RequesterEmail,
                ct: ct);
        }
    }
}