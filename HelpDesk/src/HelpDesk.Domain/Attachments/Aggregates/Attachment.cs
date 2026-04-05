using HelpDesk.Domain.Attachments.Events;
using HelpDesk.Domain.Attachments.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Attachments.Aggregates
{
    public sealed class Attachment : AggregateRoot<int>
    {
        public int TicketId { get; private set; }

        public string FileName { get; private set; } = "";
        public string ContentType { get; private set; } = "";
        public long SizeBytes { get; private set; }

        public string StorageKey { get; private set; } = "";
        public string? PublicUrl { get; private set; }

        public int UploadedById { get; private set; }
        public DateTime UploadedAt { get; private set; }

        private Attachment()
        {
        }

        private Attachment(
            int id,
            int ticketId,
            string fileName,
            string contentType,
            long sizeBytes,
            string storageKey,
            string? publicUrl,
            int uploadedById,
            DateTime uploadedAt) : base(id)
        {
            if (ticketId <= 0)
                throw new DomainException("Chamado inválido.");

            if (uploadedById <= 0)
                throw new DomainException("Usuário inválido.");

            TicketId = ticketId;
            FileName = fileName;
            ContentType = contentType;
            SizeBytes = sizeBytes;
            StorageKey = storageKey;
            PublicUrl = publicUrl;
            UploadedById = uploadedById;
            UploadedAt = uploadedAt;
        }

        public static Attachment CreateNew(
            int ticketId,
            AttachmentFile file,
            StorageKey storageKey,
            string? publicUrl,
            int uploadedById,
            DateTime now)
        {
            return new Attachment(
                id: 0,
                ticketId: ticketId,
                fileName: file.FileName,
                contentType: file.ContentType,
                sizeBytes: file.SizeBytes,
                storageKey: storageKey.Value,
                publicUrl: publicUrl,
                uploadedById: uploadedById,
                uploadedAt: now);
        }

        public static Attachment Rehydrate(
            int id,
            int ticketId,
            string fileName,
            string contentType,
            long sizeBytes,
            string storageKey,
            string? publicUrl,
            int uploadedById,
            DateTime uploadedAt)
        {
            return new Attachment(
                id: id,
                ticketId: ticketId,
                fileName: fileName,
                contentType: contentType,
                sizeBytes: sizeBytes,
                storageKey: storageKey,
                publicUrl: publicUrl,
                uploadedById: uploadedById,
                uploadedAt: uploadedAt);
        }

        public void EnsureCanBeDeletedBy(int requestedByUserId)
        {
            if (requestedByUserId <= 0)
                throw new DomainException("Usuário inválido.");

            if (UploadedById != requestedByUserId)
                throw new DomainException("Não é possível excluir anexos de outras pessoas!");
        }

        public void RaiseAddedEvent(string actorUserName, DateTime now)
        {
            Raise(AttachmentUploadedDomainEvent.Create(
                attachmentId: Id,
                ticketId: TicketId,
                authorId: UploadedById,
                actorUserName: actorUserName,
                fileName: FileName,
                contentType: ContentType,
                extension: GetExtension(),
                storageKey: StorageKey,
                occurredAt: now));
        }

        public void RaiseDeletedEvent(string actorUserName, DateTime now)
        {
            Raise(AttachmentDeletedDomainEvent.Create(
                attachmentId: Id,
                ticketId: TicketId,
                authorId: UploadedById,
                actorUserName: actorUserName,
                fileName: FileName,
                contentType: ContentType,
                extension: GetExtension(),
                storageKey: StorageKey,
                occurredAt: now));
        }

        private string GetExtension()
        {
            var extension = Path.GetExtension(FileName);
            return string.IsNullOrWhiteSpace(extension) ? "(sem extensão)" : extension;
        }
    }
}