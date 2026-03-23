using HelpDesk.Domain.Ticketing.Enums;

namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed class CreateTicketDto
    {
        public string Title { get; init; } = "";
        public string Description { get; init; } = "";
        public string Priority { get; init; } = TicketPriority.Media;
        public int CategoryId { get; init; }
    }
}
