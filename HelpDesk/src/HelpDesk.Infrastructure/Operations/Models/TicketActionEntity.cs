using HelpDesk.Infrastructure.Ticketing.Models;

namespace HelpDesk.Infrastructure.Operations.Models
{
    public sealed class TicketActionEntity
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public TicketEntity Ticket { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
