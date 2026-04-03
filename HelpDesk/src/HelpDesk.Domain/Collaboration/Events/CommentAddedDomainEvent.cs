using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Collaboration.Events
{
    public sealed record CommentAddedDomainEvent(
        int TicketId,
        int AuthorId,
        string ActorUserName,
        string Visibility,
        string Message,
        Guid EventId,
        DateTime OccurredAt)
        : DomainEvent(EventId, OccurredAt)
    {
        public static CommentAddedDomainEvent Create(
            int ticketId,
            int authorId,
            string actorUserName,
            string visibility,
            string message,
            DateTime occurredAt)
            => new(
                ticketId,
                authorId,
                actorUserName,
                visibility,
                message,
                Guid.NewGuid(),
                occurredAt);
    }
}