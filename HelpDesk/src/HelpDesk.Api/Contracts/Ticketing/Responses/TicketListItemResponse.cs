using HelpDesk.Api.Contracts.Shared;

namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class TicketListItemResponse
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? SlaDueAt { get; init; }

        public UserMiniResponse Requester { get; init; } = new();
        public UserMiniResponse? Assignee { get; init; }
        public CategoryMiniResponse Category { get; init; } = new();
    }
}