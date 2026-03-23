using HelpDesk.Application.Attachments.DTOs;
using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Shared.DTOs;

namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record TicketDetailsDto(
        int Id,
        string Title,
        string Description,
        string Status,
        string Priority,
        DateTime CreatedAt,
        DateTime? AssignedAt,
        DateTime? ClosedAt,
        DateTime SlaStartAt,
        DateTime? SlaDueAt,
        UserMiniDto Requester,
        UserMiniDto? Assignee,
        CategoryMiniDto Category,
        List<CommentDetailsDto> Comments,
        List<AttachmentListItemDto> Attachments,
        List<TicketActionDto> Actions
    );
}
