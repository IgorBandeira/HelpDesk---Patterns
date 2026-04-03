using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.Ticketing.UseCases.ChangeStatus
{
    public sealed class ChangeStatusHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ChangeStatusHandler(
            ITicketRepository tickets,
            IUserReadPort users,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            => (_tickets, _users, _clock, _domainEventDispatcher) =
               (tickets, users, clock, domainEventDispatcher);

        public async Task<ChangeStatusResponseDto> HandleAsync(ChangeStatusCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var t = await _tickets.GetByIdAsync(cmd.Id);
            if (t is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            var cur = t.Status;
            var next = cmd.Dto.NewStatus;
            var now = _clock.Now;

            try
            {
                t.ChangeStatus(next, actorUserId: user.Id, now: now);
                t.RaiseStatusChangedEvent(cur, t.Status, user.Name, now);
            }
            catch (DomainException ex)
            {
                if (ex.Message.Contains("Usuário não permitido", StringComparison.OrdinalIgnoreCase))
                    throw new AppException(HttpStatusCodes.Forbidden, ex.Message);

                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            await _tickets.SaveAsync(t);

            await _domainEventDispatcher.DispatchAsync(t.DomainEvents, ct);
            t.ClearDomainEvents();

            return new ChangeStatusResponseDto(t.Id, cur, t.Status, t.ClosedAt);
        }
    }
}