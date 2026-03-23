namespace HelpDesk.Application.Ticketing.UseCases.ListTickets
{
    public sealed record ListTicketsQuery(
        string? Status, string? Priority, string? Title,
        DateTime? CreatedFrom, DateTime? CreatedTo,
        int? RequesterId, int? AssigneeId, int? CategoryId,
        DateTime? SlaDueFrom, DateTime? SlaDueTo,
        bool OverdueOnly, int Page, int PageSize);
}