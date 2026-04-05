using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Attachments.Events
{
    public sealed record AttachmentDeletedDomainEvent(
        int AttachmentId,
        int TicketId,
        int AuthorId,
        string ActorUserName,
        string FileName,
        string ContentType,
        string Extension,
        string StorageKey,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static AttachmentDeletedDomainEvent Create(
            int attachmentId,
            int ticketId,
            int authorId,
            string actorUserName,
            string fileName,
            string contentType,
            string extension,
            string storageKey,
            DateTime occurredAt)
            => new(
                attachmentId,
                ticketId,
                authorId,
                actorUserName,
                fileName,
                contentType,
                extension,
                storageKey,
                Guid.NewGuid(),
                occurredAt);
    }
}