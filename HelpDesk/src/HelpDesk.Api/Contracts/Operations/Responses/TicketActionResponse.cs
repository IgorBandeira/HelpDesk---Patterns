namespace HelpDesk.Api.Contracts.Operations.Responses
{
    public sealed class TicketActionResponse
    {
        public string Description { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
