namespace HelpDesk.Application.IdentityAccess.Ports
{
    public sealed record UserSnapshot(int Id, string Name, string Email, string Role);

    public interface IUserReadPort
    {
        Task<UserSnapshot?> GetByIdAsync(int id);
    }
}
