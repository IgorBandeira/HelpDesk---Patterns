namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class RequesterResponse
    {
        public int TicketId { get; init; }
        public string Status { get; init; } = string.Empty;
        public int RequesterId { get; init; }
        public string RequesterName { get; init; } = string.Empty;
    }
}