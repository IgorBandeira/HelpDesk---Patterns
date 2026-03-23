using HelpDesk.Api.Contracts.Attachments.Responses;
using HelpDesk.Api.Contracts.Collaboration.Responses;
using HelpDesk.Api.Contracts.Shared;

namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class TicketDetailsResponse
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? AssignedAt { get; init; }
        public DateTime? ClosedAt { get; init; }
        public DateTime SlaStartAt { get; init; }
        public DateTime? SlaDueAt { get; init; }

        public UserMiniResponse Requester { get; init; } = new();
        public UserMiniResponse? Assignee { get; init; }
        public CategoryMiniResponse Category { get; init; } = new();

        public List<CommentDetailsResponse> Comments { get; init; } = new();
        public List<AttachmentItemResponse> Attachments { get; init; } = new();
        public List<TicketActionResponse> Actions { get; init; } = new();
    }
}