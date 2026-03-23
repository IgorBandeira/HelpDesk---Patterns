namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record TicketResponseDto(
        int Id,
        string Title,
        string Description,
        string Status,
        string Priority,
        DateTime CreatedAt,
        DateTime SlaStartAt,
        DateTime? SlaDueAt,
        int RequesterId,
        int CategoryId
    );
}
