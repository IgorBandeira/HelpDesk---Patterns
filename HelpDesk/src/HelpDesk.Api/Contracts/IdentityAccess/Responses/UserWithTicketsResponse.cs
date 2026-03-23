namespace HelpDesk.Api.Contracts.IdentityAccess.Responses
{
    public sealed record UserWithTicketsResponse(
        int Id,
        string Name,
        string Email,
        string Role,
        IEnumerable<UserTicketResponse> RequestedTickets,
        IEnumerable<UserTicketResponse> AssignedTickets
    );
}
