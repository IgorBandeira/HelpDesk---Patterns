using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Collaboration.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class CommentAddedDomainEventHandler
        : IDomainEventHandler<CommentAddedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public CommentAddedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(CommentAddedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg = domainEvent.Visibility == "Interno"
                ? $"Comentário interno adicionado por {domainEvent.ActorUserName}."
                : $"Comentário adicionado por {domainEvent.ActorUserName}.";

            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, ct: ct);
        }
    }
}