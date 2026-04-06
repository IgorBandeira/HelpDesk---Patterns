using HelpDesk.Application.IdentityAccess.DTOs;
using HelpDesk.Application.IdentityAccess.Internal;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Authorization;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.IdentityAccess.Events;
using HelpDesk.Domain.IdentityAccess.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.IdentityAccess.UseCases.PatchUser
{
    public sealed class PatchUserHandler
    {
        private readonly IUserReadPort _userRead;
        private readonly IUserRepository _users;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public PatchUserHandler(
            IUserReadPort userRead,
            IUserRepository users,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _userRead = userRead;
            _users = users;
            _clock = clock;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<UserDto> HandleAsync(PatchUserCommand command, CancellationToken ct = default)
        {
            if (command.AuthUserId <= 0)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var authUser = await _userRead.GetByIdAsync(command.AuthUserId);
            if (authUser is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            AuthorizationRules.EnsureManager(authUser, "Apenas Managers podem atualizar usuários.");

            var user = await _users.GetByIdAsync(command.Id);
            if (user is null)
                throw new AppException(HttpStatusCodes.NotFound, "Usuário não encontrado.");

            var previousName = user.Name.Value;
            var previousEmail = user.Email.Value;
            var previousRole = user.Role.Value;

            if (!string.IsNullOrWhiteSpace(command.Dto.Name))
            {
                UserName newName;
                try
                {
                    newName = UserName.Create(command.Dto.Name);
                }
                catch (DomainException ex)
                {
                    throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
                }

                user.ReplaceName(newName);
            }

            if (!string.IsNullOrWhiteSpace(command.Dto.Email))
            {
                EmailAddress newEmail;
                try
                {
                    newEmail = EmailAddress.Create(command.Dto.Email);
                }
                catch (DomainException ex)
                {
                    throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
                }

                if (!string.Equals(previousEmail, newEmail.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var newEmailLower = newEmail.Value.ToLowerInvariant();
                    var emailExists = await _users.EmailExistsAsync(newEmailLower, excludingUserId: command.Id);
                    if (emailExists)
                        throw new AppException(HttpStatusCodes.Conflict, "Já existe usuário com esse e-mail.");
                }

                user.ReplaceEmail(newEmail);
            }

            if (!string.IsNullOrWhiteSpace(command.Dto.Role))
            {
                UserRole newRole;
                try
                {
                    newRole = UserRole.Create(command.Dto.Role);
                }
                catch (DomainException ex)
                {
                    throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
                }

                user.ReplaceRole(newRole);
            }

            var actionDescriptions = UserUpdateActionBuilder.Build(
                actorUserName: authUser.Name,
                previousName: previousName,
                currentName: user.Name.Value,
                previousEmail: previousEmail,
                currentEmail: user.Email.Value,
                previousRole: previousRole,
                currentRole: user.Role.Value);

            if (actionDescriptions.Count == 0)
                throw new AppException(HttpStatusCodes.BadRequest, "Nenhuma alteração detectada.");

            var changes = actionDescriptions
                .Select(description => new UserUpdatedChange(description))
                .ToList();

            var now = _clock.Now;

            user.RaiseUpdatedEvent(authUser.Name, changes, now);

            await _users.SaveAsync(user);

            await _domainEventDispatcher.DispatchAsync(user.DomainEvents, ct);
            user.ClearDomainEvents();

            return new UserDto(user.Id, user.Name.Value, user.Email.Value, user.Role.Value);
        }
    }
}