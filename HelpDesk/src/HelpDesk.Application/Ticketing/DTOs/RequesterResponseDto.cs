namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record RequesterResponseDto(
    int TicketId,
    string Status,
    int RequesterId,
    string RequesterName
    );
}
