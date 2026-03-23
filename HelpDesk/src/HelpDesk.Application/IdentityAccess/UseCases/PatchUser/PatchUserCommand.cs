using HelpDesk.Application.IdentityAccess.DTOs;

namespace HelpDesk.Application.IdentityAccess.UseCases.PatchUser
{
    public sealed record PatchUserCommand(int Id, int AuthUserId, UpdateUserDto Dto);

}
