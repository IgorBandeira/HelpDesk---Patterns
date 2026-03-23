namespace HelpDesk.Application.Ticketing.DTOs
{
    public sealed record TicketActionDto(
        string Description,
        DateTime CreatedAt
    );
}
