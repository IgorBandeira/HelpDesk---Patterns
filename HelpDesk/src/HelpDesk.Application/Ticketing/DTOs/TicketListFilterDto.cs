namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record TicketListFilterDto(
            string? Status,
            string? Priority,
            string? TitleContainsLower,
            DateTime? CreatedFrom,
            DateTime? CreatedTo,
            int? RequesterId,
            int? AssigneeId,
            int? CategoryId,
            DateTime? SlaDueFrom,
            DateTime? SlaDueTo,
            bool OverdueOnly
    );
}
