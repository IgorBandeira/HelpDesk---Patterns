namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class TicketResponse
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime SlaStartAt { get; init; }
        public DateTime? SlaDueAt { get; init; }
        public int RequesterId { get; init; }
        public int CategoryId { get; init; }
    }
}