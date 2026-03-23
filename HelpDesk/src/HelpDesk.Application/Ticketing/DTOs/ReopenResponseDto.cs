namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record ReopenResponseDto(
        int TicketId,
        string PreviousStatus,
        string NewStatus,
        DateTime ReopenedAt,
        int ActorUserId,
        string Reason
    );
}
