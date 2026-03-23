using HelpDesk.Domain.Attachments.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Attachments.Aggregates
{
    public sealed class Attachment
    {
        public int Id { get; private set; }
        public int TicketId { get; private set; }

        public string FileName { get; private set; } = "";
        public string ContentType { get; private set; } = "";
        public long SizeBytes { get; private set; }

        public string StorageKey { get; private set; } = "";
        public string? PublicUrl { get; private set; }

        public int UploadedById { get; private set; }
        public DateTime UploadedAt { get; private set; }

        private Attachment() { }

        private Attachment(
            int ticketId,
            AttachmentFile file,
            StorageKey storageKey,
            string? publicUrl,
            int uploadedById,
            DateTime now)
        {
            if (uploadedById <= 0)
                throw new DomainException("Usuário inválido.");

            TicketId = ticketId;

            FileName = file.FileName;
            ContentType = file.ContentType;
            SizeBytes = file.SizeBytes;

            StorageKey = storageKey.Value;
            PublicUrl = publicUrl;

            UploadedById = uploadedById;
            UploadedAt = now;
        }

        public static Attachment CreateNew(
            int ticketId,
            AttachmentFile file,
            StorageKey storageKey,
            string? publicUrl,
            int uploadedById,
            DateTime now)
            => new(ticketId, file, storageKey, publicUrl, uploadedById, now);
    }
}
