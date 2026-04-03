using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Collaboration.Events
{
    public sealed record CommentMessageReplacedDomainEvent(
        int CommentId,
        int TicketId,
        int AuthorId,
        string ActorUserName,
        string Visibility,
        string OldMessage,
        string NewMessage,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static CommentMessageReplacedDomainEvent Create(
            int commentId,
            int ticketId,
            int authorId,
            string actorUserName,
            string visibility,
            string oldMessage,
            string newMessage,
            DateTime occurredAt)
            => new(
                commentId,
                ticketId,
                authorId,
                actorUserName,
                visibility,
                oldMessage,
                newMessage,
                Guid.NewGuid(),
                occurredAt);
    }
}