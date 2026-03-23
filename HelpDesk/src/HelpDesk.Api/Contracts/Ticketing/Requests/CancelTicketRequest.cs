namespace HelpDesk.Api.Contracts.Ticketing.Requests
{
    public sealed class CancelTicketRequest
    {
        public string? Reason { get; init; }
    }
}