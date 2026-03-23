namespace HelpDesk.Application.IdentityAccess.DTOs
{
    public sealed record UserWithTicketsDto(
        int Id,
        string Name,
        string Email,
        string Role,
        IEnumerable<UserTicketDto> RequestedTickets,
        IEnumerable<UserTicketDto> AssignedTickets
    );
}
