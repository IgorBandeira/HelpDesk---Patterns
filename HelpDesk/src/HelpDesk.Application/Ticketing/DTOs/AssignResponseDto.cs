namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record AssignResponseDto(
        int TicketId,
        string Status,
        DateTime AssignedAt,
        int AgentId,
        string AgentName
    );
}
