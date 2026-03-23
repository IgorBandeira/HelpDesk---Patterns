namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class AssignTicketResponse
    {
        public int TicketId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime AssignedAt { get; init; }
        public int AgentId { get; init; }
        public string AgentName { get; init; } = string.Empty;
    }
}