namespace HelpDesk.Application.IdentityAccess.DTOs
{
    public sealed record UserTicketDto(int Id, string Title, string Status, string PriorityLevel);
}
