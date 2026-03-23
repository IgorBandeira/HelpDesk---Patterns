namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class ReopenTicketResponse
    {
        public int TicketId { get; init; }
        public string PreviousStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public DateTime ReopenedAt { get; init; }
        public int ActorUserId { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}