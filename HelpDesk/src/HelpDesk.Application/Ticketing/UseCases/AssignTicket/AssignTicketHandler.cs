using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Internal;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.Ticketing.UseCases.AssignTicket
{
    public sealed class AssignTicketHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public AssignTicketHandler(
            ITicketRepository tickets,
            IUserReadPort users,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            => (_tickets, _users, _clock, _domainEventDispatcher) =
               (tickets, users, clock, domainEventDispatcher);

        public async Task<AssignResponseDto> HandleAsync(AssignTicketCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var t = await _tickets.GetByIdAsync(cmd.Id);
            if (t is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (!TicketAuthRules.Owner(user, t))
                throw new AppException(HttpStatusCodes.Forbidden, "Somente o solicitante do chamado ou um Manager pode atribuir este chamado a um agent.");

            var agent = await _users.GetByIdAsync(cmd.Dto.AgentId);
            if (agent is null)
                throw new AppException(HttpStatusCodes.BadRequest, "Agent não encontrado.");

            if (!TicketAuthRules.IsAgent(agent))
                throw new AppException(HttpStatusCodes.BadRequest, $"Usuário '{agent.Name}' não é um agent e não pode ser atribuído a este chamado.");

            var now = _clock.Now;

            try
            {
                t.AssignToAgent(agent.Id, now);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            t.RaiseAssignedEvent(agent.Id, agent.Name, user.Name, now);

            await _tickets.SaveAsync(t);

            await _domainEventDispatcher.DispatchAsync(t.DomainEvents, ct);
            t.ClearDomainEvents();

            return new AssignResponseDto(t.Id, t.Status, t.AssignedAt!.Value, agent.Id, agent.Name);
        }
    }
}