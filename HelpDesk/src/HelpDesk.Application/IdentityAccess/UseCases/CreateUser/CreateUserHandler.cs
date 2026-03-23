using HelpDesk.Application.IdentityAccess.DTOs;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Authorization;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.IdentityAccess.Aggregates;
using HelpDesk.Domain.IdentityAccess.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.IdentityAccess.UseCases.CreateUser
{
    public sealed class CreateUserHandler
    {
        private readonly IUserReadPort _userRead;
        private readonly IUserRepository _users;

        public CreateUserHandler(IUserReadPort userRead, IUserRepository users)
        {
            _userRead = userRead;
            _users = users;
        }

        public async Task<UserDto> HandleAsync(CreateUserCommand command)
        {
            if (command.AuthUserId <= 0)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var authUser = await _userRead.GetByIdAsync(command.AuthUserId);
            AuthorizationRules.EnsureManager(authUser, "Apenas Managers podem inserir usuários.");

            UserName name;
            EmailAddress email;
            UserRole role;

            try
            {
                name = UserName.Create(command.Dto.Name);
                email = EmailAddress.Create(command.Dto.Email);
                role = UserRole.Create(command.Dto.Role);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            var emailLower = email.Value.Trim().ToLowerInvariant();
            var exists = await _users.EmailExistsAsync(emailLower);
            if (exists)
                throw new AppException(HttpStatusCodes.Conflict, "Já existe usuário com esse e-mail.");

            var user = User.CreateNew(name, email, role);
            await _users.AddAsync(user);

            return new UserDto(user.Id, user.Name.Value, user.Email.Value, user.Role.Value);
        }
    }
}