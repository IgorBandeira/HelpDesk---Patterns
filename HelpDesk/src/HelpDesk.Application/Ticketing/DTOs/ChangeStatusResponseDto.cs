namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record ChangeStatusResponseDto(
        int TicketId,
        string PreviousStatus,
        string NewStatus,
        DateTime? ClosedAt
    );
}
