namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record SlaMonitorTicketDto(
        int Id,
        string Status,
        DateTime SlaStartAt,
        DateTime? SlaDueAt
    );
}