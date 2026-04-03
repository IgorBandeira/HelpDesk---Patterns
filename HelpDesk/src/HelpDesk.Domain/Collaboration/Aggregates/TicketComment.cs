using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Collaboration.Events;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.Collaboration.Aggregates
{
    public sealed class TicketComment : AggregateRoot<int>
    {
        public int TicketId { get; private set; }
        public int AuthorId { get; private set; }
        public string Visibility { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        private TicketComment()
        {
        }

        private TicketComment(
            int id,
            int ticketId,
            int authorId,
            string visibility,
            string message,
            DateTime createdAt) : base(id)
        {
            TicketId = ticketId;
            AuthorId = authorId;
            Visibility = visibility;
            Message = message;
            CreatedAt = createdAt;
        }

        public static TicketComment CreateNew(
            int ticketId,
            int authorId,
            string visibility,
            CommentMessage message,
            DateTime now)
        {
            EnsureVisibilityIsValid(visibility);

            return new TicketComment(
                id: 0,
                ticketId: ticketId,
                authorId: authorId,
                visibility: visibility,
                message: message.Value,
                createdAt: now);
        }

        public static TicketComment Rehydrate(
            int id,
            int ticketId,
            int authorId,
            string visibility,
            string message,
            DateTime createdAt)
        {
            EnsureVisibilityIsValid(visibility);

            return new TicketComment(
                id: id,
                ticketId: ticketId,
                authorId: authorId,
                visibility: visibility,
                message: message,
                createdAt: createdAt);
        }

        public void ReplaceMessage(CommentMessage newMessage, DateTime now)
        {
            var currentNormalized = (Message ?? string.Empty).Trim();

            if (string.Equals(currentNormalized, newMessage.Value, StringComparison.Ordinal))
                throw new DomainException("Não houve mudança.");

            Message = $"editado: {newMessage.Value}";
            CreatedAt = now;
        }

        public void RaiseAddedEvent(string actorUserName, DateTime now)
        {
            Raise(CommentAddedDomainEvent.Create(
                ticketId: TicketId,
                authorId: AuthorId,
                actorUserName: actorUserName,
                visibility: Visibility,
                message: Message,
                occurredAt: now));
        }

        public void RaiseMessageReplacedEvent(string actorUserName, string oldMessage, DateTime now)
        {
            Raise(CommentMessageReplacedDomainEvent.Create(
                commentId: Id,
                ticketId: TicketId,
                authorId: AuthorId,
                actorUserName: actorUserName,
                visibility: Visibility,
                oldMessage: oldMessage,
                newMessage: Message,
                occurredAt: now));
        }

        public void RaiseDeletedEvent(string actorUserName, DateTime now)
        {
            Raise(CommentDeletedDomainEvent.Create(
                commentId: Id,
                ticketId: TicketId,
                authorId: AuthorId,
                actorUserName: actorUserName,
                visibility: Visibility,
                message: Message,
                occurredAt: now));
        }

        private static void EnsureVisibilityIsValid(string visibility)
        {
            if (visibility != CommentVisibility.Public && visibility != CommentVisibility.Internal)
                throw new DomainException("Visibilidade inválida. Use 'Público' ou 'Interno'.");
        }
    }
}