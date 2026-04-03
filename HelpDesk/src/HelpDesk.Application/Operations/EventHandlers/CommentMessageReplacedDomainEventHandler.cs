using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Collaboration.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class CommentMessageReplacedDomainEventHandler
        : IDomainEventHandler<CommentMessageReplacedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public CommentMessageReplacedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(CommentMessageReplacedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg = domainEvent.Visibility == "Interno"
                ? $"Comentário interno editado por {domainEvent.ActorUserName}."
                : $"Comentário editado por {domainEvent.ActorUserName}.";

            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, ct: ct);
        }
    }
}