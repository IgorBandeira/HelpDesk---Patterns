using HelpDesk.Application.IdentityAccess.DTOs;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.Application.IdentityAccess.UseCases.GetUserById
{
    public sealed class GetUserByIdHandler
    {
        private readonly IUserRepository _users;
        private readonly ITicketUserQueryPort _tickets;

        public GetUserByIdHandler(IUserRepository users, ITicketUserQueryPort tickets)
        {
            _users = users;
            _tickets = tickets;
        }

        public async Task<UserWithTicketsDto> HandleAsync(GetUserByIdQuery query)
        {
            var user = await _users.GetByIdNoTrackingAsync(query.Id);
            if (user is null)
                throw new AppException(HttpStatusCodes.NotFound, "Usuário não encontrado.");

            var requested = await _tickets.ListRequestedTicketsAsync(query.Id);
            var assigned = await _tickets.ListAssignedTicketsAsync(query.Id);

            return new UserWithTicketsDto(
                user.Id,
                user.Name.Value,
                user.Email.Value,
                user.Role.Value,
                requested.Select(t => new UserTicketDto(t.Id, t.Title, t.Status, t.PriorityLevel)),
                assigned.Select(t => new UserTicketDto(t.Id, t.Title, t.Status, t.PriorityLevel))
            );
        }
    }
}
