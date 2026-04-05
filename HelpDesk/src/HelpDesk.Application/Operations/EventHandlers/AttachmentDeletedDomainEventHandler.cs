using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.Operations.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Domain.Attachments.Events;

namespace HelpDesk.Application.Operations.EventHandlers
{
    public sealed class AttachmentDeletedDomainEventHandler
        : IDomainEventHandler<AttachmentDeletedDomainEvent>
    {
        private readonly INotificationPort _notify;
        private readonly IFileStoragePort _storage;

        public AttachmentDeletedDomainEventHandler(
            INotificationPort notify,
            IFileStoragePort storage)
        {
            _notify = notify;
            _storage = storage;
        }

        public async Task HandleAsync(AttachmentDeletedDomainEvent domainEvent, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(domainEvent.StorageKey))
                await _storage.DeleteAsync(domainEvent.StorageKey);

            var msg =
                $"Anexo \"{domainEvent.FileName}\" excluído por {domainEvent.ActorUserName}.";

            await _notify.NotifyTicketActionAsync(domainEvent.TicketId, msg, ct: ct);
        }
    }
}