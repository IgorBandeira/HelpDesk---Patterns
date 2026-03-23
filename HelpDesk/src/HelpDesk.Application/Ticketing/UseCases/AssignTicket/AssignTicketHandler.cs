using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Internal;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Ticketing.Enums;

namespace HelpDesk.Application.Ticketing.UseCases.AssignTicket
{
    public sealed class AssignTicketHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly INotificationPort _notify;

        public AssignTicketHandler(ITicketRepository tickets, IUserReadPort users, IClock clock, INotificationPort notify)
            => (_tickets, _users, _clock, _notify) = (tickets, users, clock, notify);

        public async Task<AssignResponseDto> HandleAsync(AssignTicketCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var t = await _tickets.GetByIdAsync(cmd.Id);
            if (t is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (t.Status is TicketStatus.Fechado or TicketStatus.Cancelado)
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível atribuir tickets inativos.");

            if (!TicketAuthRules.Owner(user, t))
                throw new AppException(HttpStatusCodes.Forbidden, "Somente o solicitante do chamado ou um Manager pode atribuir este chamado a um agent.");

            var agent = await _users.GetByIdAsync(cmd.Dto.AgentId);
            if (agent is null)
                throw new AppException(HttpStatusCodes.BadRequest, "Agent não encontrado.");

            if (!TicketAuthRules.IsAgent(agent))
                throw new AppException(HttpStatusCodes.BadRequest, $"Usuário '{agent.Name}' não é um agent e não pode ser atribuído a este chamado.");

            var now = _clock.Now;

            t.AssignToAgent(agent.Id, now);

            await _tickets.SaveAsync(t);

            var msg = $"Chamado atribuído para agent {agent.Name} por {user.Name}.";
            await _notify.NotifyTicketActionAsync(t.Id, msg, extraEmail: agent.Email, ct: ct);

            return new AssignResponseDto(t.Id, t.Status, t.AssignedAt!.Value, agent.Id, agent.Name);
        }
    }
}
