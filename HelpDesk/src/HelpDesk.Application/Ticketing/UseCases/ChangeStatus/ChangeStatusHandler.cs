using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.Ticketing.UseCases.ChangeStatus
{
    public sealed class ChangeStatusHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly INotificationPort _notify;

        public ChangeStatusHandler(ITicketRepository tickets, IUserReadPort users, IClock clock, INotificationPort notify)
            => (_tickets, _users, _clock, _notify) = (tickets, users, clock, notify);

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

            try
            {
                t.ChangeStatus(next, actorUserId: user.Id, now: _clock.Now);
            }
            catch (DomainException ex)
            {
                if (ex.Message.Contains("Usuário não permitido", StringComparison.OrdinalIgnoreCase))
                    throw new AppException(HttpStatusCodes.Forbidden, ex.Message);

                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            await _tickets.SaveAsync(t);

            var msg = $"Status do Chamado atualizado de: '{cur}' para: '{t.Status}' por {user.Name}.";
            await _notify.NotifyTicketActionAsync(t.Id, msg, null, ct);

            return new ChangeStatusResponseDto(t.Id, cur, t.Status, t.ClosedAt);
        }
    }
}
