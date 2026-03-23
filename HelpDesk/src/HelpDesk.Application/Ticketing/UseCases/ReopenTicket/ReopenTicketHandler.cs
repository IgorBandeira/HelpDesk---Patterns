using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Internal;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.Application.Ticketing.UseCases.ReopenTicket
{
    public sealed class ReopenTicketHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly INotificationPort _notify;

        public ReopenTicketHandler(ITicketRepository tickets, IUserReadPort users, IClock clock, INotificationPort notify)
            => (_tickets, _users, _clock, _notify) = (tickets, users, clock, notify);

        public async Task<ReopenResponseDto> HandleAsync(ReopenTicketCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var t = await _tickets.GetByIdAsync(cmd.Id);
            if (t is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (!TicketAuthRules.Owner(user, t))
                throw new AppException(HttpStatusCodes.Forbidden, "Somente o solicitante do chamado ou um Manager pode reabrí-lo.");

            var reason = cmd.Dto.Reason?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(reason))
                throw new AppException(HttpStatusCodes.BadRequest, "O motivo da reabertura é obrigatório.");

            if (t.Status is not ("Resolvido" or "Fechado"))
                throw new AppException(HttpStatusCodes.BadRequest, "Só reabre chamado se estiver Resolvido ou Fechado.");

            var previous = t.Status;
            var now = _clock.Now;

            t.Reopen(reason, now);

            await _tickets.SaveAsync(t);

            var msg = $"Chamado reaberto por {user.Name}.";
            await _notify.NotifyTicketActionAsync(t.Id, msg, null, ct);

            return new ReopenResponseDto(t.Id, previous, t.Status, now, user.Id, reason);
        }
    }
}
