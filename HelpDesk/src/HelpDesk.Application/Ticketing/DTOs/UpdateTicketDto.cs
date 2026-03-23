namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed class UpdateTicketDto
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public string? Priority { get; init; }
        public int? CategoryId { get; init; }
    }
}
