using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Attachments.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class AttachmentUploadedDomainEventHandler
        : IDomainEventHandler<AttachmentUploadedDomainEvent>
    {
        private readonly INotificationPort _notify;

        public AttachmentUploadedDomainEventHandler(INotificationPort notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(AttachmentUploadedDomainEvent domainEvent, CancellationToken ct = default)
        {
            var msg =
                $"Anexo \"{domainEvent.FileName}\" enviado por {domainEvent.ActorUserName}.";

            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, ct: ct);
        }
    }
}