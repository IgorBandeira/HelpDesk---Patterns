namespace HelpDesk.Application.Operations.DTOs
{
    public sealed record TicketActionDto(
        string Description,
        DateTime CreatedAt
    );
}
