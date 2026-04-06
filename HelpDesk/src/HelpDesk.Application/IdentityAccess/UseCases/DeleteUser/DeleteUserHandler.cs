using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Authorization;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.Application.IdentityAccess.UseCases.DeleteUser
{
    public sealed class DeleteUserHandler
    {
        private readonly IUserReadPort _userRead;
        private readonly IUserRepository _users;
        private readonly ITicketUserQueryPort _tickets;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public DeleteUserHandler(
            IUserReadPort userRead,
            IUserRepository users,
            ITicketUserQueryPort tickets,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _userRead = userRead;
            _users = users;
            _tickets = tickets;
            _clock = clock;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task HandleAsync(DeleteUserCommand command, CancellationToken ct = default)
        {
            if (command.AuthUserId <= 0)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var authUser = await _userRead.GetByIdAsync(command.AuthUserId);
            if (authUser is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            AuthorizationRules.EnsureManager(authUser, "Apenas Managers podem excluir usuários.");

            var user = await _users.GetByIdAsync(command.Id);
            if (user is null)
                throw new AppException(HttpStatusCodes.NotFound, "Usuário não encontrado.");

            var hasActiveAsAssignee = await _tickets.HasActiveTicketsAsAssigneeAsync(command.Id);
            if (hasActiveAsAssignee)
                throw new AppException(HttpStatusCodes.Conflict, "Usuário possui tickets ativos vinculados como agent!");

            var hasActiveAsRequester = await _tickets.HasActiveTicketsAsRequesterAsync(command.Id);
            if (hasActiveAsRequester)
                throw new AppException(HttpStatusCodes.Conflict, "Usuário possui tickets ativos vinculados como requester!");

            user.RaiseDeletedEvent(authUser.Name, _clock.Now);

            await _users.DeleteAsync(user);

            await _domainEventDispatcher.DispatchAsync(user.DomainEvents, ct);
            user.ClearDomainEvents();
        }
    }
}