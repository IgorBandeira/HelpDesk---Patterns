namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record CancelResponseDto(
        int TicketId,
        string PreviousStatus,
        string NewStatus,
        DateTime ClosedAt,
        int ActorUserId,
        string Reason
    );
}
