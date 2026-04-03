using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Collaboration.Events
{
    public sealed record CommentDeletedDomainEvent(
        int CommentId,
        int TicketId,
        int AuthorId,
        string ActorUserName,
        string Visibility,
        string Message,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static CommentDeletedDomainEvent Create(
            int commentId,
            int ticketId,
            int authorId,
            string actorUserName,
            string visibility,
            string message,
            DateTime occurredAt)
            => new(
                commentId,
                ticketId,
                authorId,
                actorUserName,
                visibility,
                message,
                Guid.NewGuid(),
                occurredAt);
    }
}