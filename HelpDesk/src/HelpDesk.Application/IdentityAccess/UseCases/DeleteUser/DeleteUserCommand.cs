namespace HelpDesk.Application.IdentityAccess.UseCases.DeleteUser
{
    public sealed record DeleteUserCommand(int Id, int AuthUserId);
}
