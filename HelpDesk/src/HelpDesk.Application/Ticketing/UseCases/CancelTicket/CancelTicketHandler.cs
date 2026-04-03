using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Internal;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.Ticketing.UseCases.CancelTicket
{
    public sealed class CancelTicketHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public CancelTicketHandler(
            ITicketRepository tickets,
            IUserReadPort users,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            => (_tickets, _users, _clock, _domainEventDispatcher) =
               (tickets, users, clock, domainEventDispatcher);

        public async Task<CancelResponseDto> HandleAsync(CancelTicketCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var t = await _tickets.GetByIdAsync(cmd.Id);
            if (t is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (!TicketAuthRules.Owner(user, t))
                throw new AppException(HttpStatusCodes.Forbidden, "Somente o solicitante do chamado ou um Manager pode cancelá-lo.");

            var reason = cmd.Dto.Reason?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(reason))
                throw new AppException(HttpStatusCodes.BadRequest, "O motivo do cancelamento é obrigatório.");

            var previous = t.Status;
            var now = _clock.Now;

            try
            {
                t.Cancel(user.Id, reason, now);
                t.RaiseCanceledEvent(user.Id, user.Name, reason, now);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            await _tickets.SaveAsync(t);

            await _domainEventDispatcher.DispatchAsync(t.DomainEvents, ct);
            t.ClearDomainEvents();

            return new CancelResponseDto(t.Id, previous, t.Status, t.ClosedAt!.Value, user.Id, reason);
        }
    }
}