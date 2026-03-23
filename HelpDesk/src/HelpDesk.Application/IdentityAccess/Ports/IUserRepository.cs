using HelpDesk.Domain.IdentityAccess.Aggregates;

namespace HelpDesk.Application.IdentityAccess.Ports
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByIdNoTrackingAsync(int id);

        Task<bool> EmailExistsAsync(string emailLower, int? excludingUserId = null);

        Task AddAsync(User user);
        Task SaveAsync(User user);
        Task DeleteAsync(User user);

        Task<IReadOnlyList<UserListItem>> ListAsync(
            string? role,
            string? emailContainsLower,
            string? nameContainsLower,
            int skip,
            int take);
    }

    public sealed record UserListItem(int Id, string Name, string Email, string Role);
}
