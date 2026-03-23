namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class TicketActionResponse
    {
        public string Description { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}