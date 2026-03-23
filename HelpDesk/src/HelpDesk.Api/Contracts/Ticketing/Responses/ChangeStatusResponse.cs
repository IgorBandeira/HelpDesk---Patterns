namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class ChangeStatusResponse
    {
        public int TicketId { get; init; }
        public string PreviousStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public DateTime? ClosedAt { get; init; }
    }
}