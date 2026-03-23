using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Internal;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Ticketing.Enums;

namespace HelpDesk.Application.Ticketing.UseCases.ChangeRequester
{
    public sealed class ChangeRequesterHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly INotificationPort _notify;

        public ChangeRequesterHandler(ITicketRepository tickets, IUserReadPort users, IClock clock, INotificationPort notify)
            => (_tickets, _users, _clock, _notify) = (tickets, users, clock, notify);

        public async Task<RequesterResponseDto> HandleAsync(ChangeRequesterCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var t = await _tickets.GetByIdAsync(cmd.Id);
            if (t is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (t.Status is TicketStatus.Fechado or TicketStatus.Cancelado)
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível atribuir chamados inativos.");

            if (!TicketAuthRules.Owner(user, t))
                throw new AppException(HttpStatusCodes.Forbidden, "Somente o solicitante do chamado ou um Manager pode atribuir este chamado a outro responsável.");

            var requester = await _users.GetByIdAsync(cmd.Dto.RequesterId);
            if (requester is null)
                throw new AppException(HttpStatusCodes.BadRequest, "Usuário não encontrado.");

            if (!TicketAuthRules.IsRequester(requester))
                throw new AppException(HttpStatusCodes.BadRequest, $"Usuário '{requester.Name}' não é um Requester e não pode ser atribuído a este chamado.");

            t.ChangeRequester(requester.Id);

            await _tickets.SaveAsync(t);

            var msg = $"{user.Name} mudou requester para {requester.Name}.";
            await _notify.NotifyTicketActionAsync(t.Id, msg, extraEmail: requester.Email, ct: ct);

            return new RequesterResponseDto(t.Id, t.Status, requester.Id, requester.Name);
        }
    }
}
