using HelpDesk.Application.IdentityAccess.DTOs;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Authorization;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.IdentityAccess.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.IdentityAccess.UseCases.PatchUser
{
    public sealed class PatchUserHandler
    {
        private readonly IUserReadPort _userRead;
        private readonly IUserRepository _users;

        public PatchUserHandler(IUserReadPort userRead, IUserRepository users)
        {
            _userRead = userRead;
            _users = users;
        }

        public async Task<UserDto> HandleAsync(PatchUserCommand command)
        {
            if (command.AuthUserId <= 0)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var authUser = await _userRead.GetByIdAsync(command.AuthUserId);
            AuthorizationRules.EnsureManager(authUser, "Apenas Managers podem atualizar usuários.");

            var user = await _users.GetByIdAsync(command.Id);
            if (user is null)
                throw new AppException(HttpStatusCodes.NotFound, "Usuário não encontrado.");

            bool changed = false;

            if (!string.IsNullOrWhiteSpace(command.Dto.Name))
            {
                var newName = UserName.Create(command.Dto.Name);
                if (!string.Equals(user.Name.Value, newName.Value, StringComparison.Ordinal))
                {
                    user.UpdateName(newName);
                    changed = true;
                }
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

                if (!string.Equals(user.Email.Value, newEmail.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var newEmailLower = newEmail.Value.ToLowerInvariant();
                    var emailExists = await _users.EmailExistsAsync(newEmailLower, excludingUserId: command.Id);
                    if (emailExists)
                        throw new AppException(HttpStatusCodes.Conflict, "Já existe usuário com esse e-mail.");

                    user.UpdateEmail(newEmail);
                    changed = true;
                }
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

                if (!string.Equals(user.Role.Value, newRole.Value, StringComparison.OrdinalIgnoreCase))
                {
                    user.UpdateRole(newRole);
                    changed = true;
                }
            }

            if (!changed)
                throw new AppException(HttpStatusCodes.BadRequest, "Nenhuma alteração detectada.");

            await _users.SaveAsync(user);

            return new UserDto(user.Id, user.Name.Value, user.Email.Value, user.Role.Value);
        }
    }
}