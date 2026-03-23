using HelpDesk.Application.Collaboration.Internal;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Collaboration.Enums;

namespace HelpDesk.Application.Ticketing.UseCases.GetTicketById
{
    public sealed class GetTicketByIdHandler
    {
        private readonly IUserReadPort _users;
        private readonly ITicketQueryPort _tickets;

        public GetTicketByIdHandler(IUserReadPort users, ITicketQueryPort tickets)
            => (_users, _tickets) = (users, tickets);

        public async Task<TicketDetailsDto> HandleAsync(GetTicketByIdQuery q)
        {
            var user = await _users.GetByIdAsync(q.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var ticket = await _tickets.GetDetailsByIdAsync(q.Id);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            var canSeeInternal = CollaborationRules.Participants(
                user.Id,
                user.Role,
                ticket.Requester.Id,
                ticket.Assignee?.Id);

            if (canSeeInternal)
                return ticket;

            var filtered = ticket.Comments
                .Where(c => c.Visibility != CommentVisibility.Internal)
                .ToList();

            return ticket with { Comments = filtered };
        }
    }
}