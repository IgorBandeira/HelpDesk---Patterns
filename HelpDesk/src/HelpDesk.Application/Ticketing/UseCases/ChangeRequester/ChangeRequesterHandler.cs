using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Internal;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.Ticketing.Enums;

namespace HelpDesk.Application.Ticketing.UseCases.ChangeRequester
{
    public sealed class ChangeRequesterHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ChangeRequesterHandler(
            ITicketRepository tickets,
            IUserReadPort users,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            => (_tickets, _users, _clock, _domainEventDispatcher) =
               (tickets, users, clock, domainEventDispatcher);

        public async Task<RequesterResponseDto> HandleAsync(ChangeRequesterCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var t = await _tickets.GetByIdAsync(cmd.Id);
            if (t is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (!TicketAuthRules.Owner(user, t))
                throw new AppException(HttpStatusCodes.Forbidden, "Somente o solicitante do chamado ou um Manager pode atribuir este chamado a outro responsável.");

            var requester = await _users.GetByIdAsync(cmd.Dto.RequesterId);
            if (requester is null)
                throw new AppException(HttpStatusCodes.BadRequest, "Usuário não encontrado.");

            if (!TicketAuthRules.IsRequester(requester))
                throw new AppException(HttpStatusCodes.BadRequest, $"Usuário '{requester.Name}' não é um Requester e não pode ser atribuído a este chamado.");

            var now = _clock.Now;

            try
            {
                t.ChangeRequester(requester.Id);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            t.RaiseRequesterChangedEvent(
                requester.Id,
                requester.Name,
                user.Name,
                requester.Email,
                now);

            await _tickets.SaveAsync(t);

            await _domainEventDispatcher.DispatchAsync(t.DomainEvents, ct);
            t.ClearDomainEvents();

            return new RequesterResponseDto(t.Id, t.Status, requester.Id, requester.Name);
        }
    }
}