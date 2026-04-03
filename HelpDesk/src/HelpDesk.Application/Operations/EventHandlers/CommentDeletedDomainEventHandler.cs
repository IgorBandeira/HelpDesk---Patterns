using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Collaboration.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class CommentDeletedDomainEventHandler
        : IDomainEventHandler<CommentDeletedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public CommentDeletedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(CommentDeletedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg = domainEvent.Visibility == "Interno"
                ? $"Comentário interno excluído por {domainEvent.ActorUserName}."
                : $"Comentário excluído por {domainEvent.ActorUserName}.";

            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, ct: ct);
        }
    }
}