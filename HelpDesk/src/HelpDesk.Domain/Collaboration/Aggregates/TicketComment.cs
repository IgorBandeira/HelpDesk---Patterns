using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Collaboration.Aggregates
{
    public sealed class TicketComment
    {
        public int Id { get; private set; }
        public int TicketId { get; private set; }
        public int AuthorId { get; private set; }
        public string Visibility { get; private set; }
        public string Message { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private TicketComment(int ticketId, int authorId, string visibility, string message, DateTime createdAt)
        {
            TicketId = ticketId;
            AuthorId = authorId;
            Visibility = visibility;
            Message = message;
            CreatedAt = createdAt;
        }

        public static TicketComment CreateNew(int ticketId, int authorId, string visibility, CommentMessage message, DateTime now)
        {
            if (visibility != CommentVisibility.Public && visibility != CommentVisibility.Internal)
                throw new DomainException("Visibilidade inválida. Use 'Público' ou 'Interno'.");

            return new TicketComment(ticketId, authorId, visibility, message.Value, now);
        }

        public void ReplaceMessage(CommentMessage newMessage, DateTime now)
        {
            var currentNormalized = (Message ?? string.Empty).Trim();

            if (string.Equals(currentNormalized, newMessage.Value, StringComparison.Ordinal))
                throw new DomainException("Não houve mudança.");

            Message = $"editado: {newMessage.Value}";
            CreatedAt = now;
        }
    }
}
