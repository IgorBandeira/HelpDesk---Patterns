using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Ticketing.Events;
using HelpDesk.Application.IdentityAccess.Ports;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class TicketAssignedDomainEventHandler
        : IDomainEventHandler<TicketAssignedDomainEvent>
    {
        private readonly INotificationPort _notify;
        private readonly IUserReadPort _users;

        public TicketAssignedDomainEventHandler(
            INotificationPort notify,
            IUserReadPort users)
        {
            _notify = notify;
            _users = users;
        }

        public async Task HandleAsync(TicketAssignedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var agent = await _users.GetByIdAsync(domainEvent.AgentId);
            var extraEmail = agent?.Email;

            var msg = $"Chamado atribuído para agent {domainEvent.AgentName} por {domainEvent.PerformedByUserName}.";
            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, extraEmail: extraEmail, ct: ct);
        }
    }
}