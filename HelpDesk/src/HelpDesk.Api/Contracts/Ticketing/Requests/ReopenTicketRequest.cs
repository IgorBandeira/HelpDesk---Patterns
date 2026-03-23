namespace HelpDesk.Api.Contracts.Ticketing.Requests
{
    public sealed class ReopenTicketRequest
    {
        public string? Reason { get; init; }
    }
}