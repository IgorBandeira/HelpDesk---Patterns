using HelpDesk.Infrastructure.IdentityAccess.Models;
using HelpDesk.Infrastructure.Ticketing.Models;

namespace HelpDesk.Infrastructure.Collaboration.Models
{
    public sealed class CommentEntity
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int? AuthorId { get; set; }
        public string Visibility { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public TicketEntity Ticket { get; set; } = null!;
        public UserEntity? Author { get; set; }
    }
}