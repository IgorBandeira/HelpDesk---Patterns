using HelpDesk.Application.Shared.DTOs;

namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record TicketListItemDto(
        int Id,
        string Title,
        string Status,
        string Priority,
        DateTime CreatedAt,
        DateTime? SlaDueAt,
        UserMiniDto Requester,
        UserMiniDto? Assignee,
        CategoryMiniDto Category
    );
}