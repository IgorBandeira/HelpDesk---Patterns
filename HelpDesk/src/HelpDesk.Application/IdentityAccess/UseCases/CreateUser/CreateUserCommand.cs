using HelpDesk.Application.IdentityAccess.DTOs;

namespace HelpDesk.Application.IdentityAccess.UseCases.CreateUser
{
    public sealed record CreateUserCommand(int AuthUserId, CreateUserDto Dto);
}
