using HelpDesk.Application.IdentityAccess.DTOs;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.IdentityAccess.Rules;

namespace HelpDesk.Application.IdentityAccess.UseCases.ListUsers
{
    public sealed class ListUsersHandler
    {
        private readonly IUserRepository _users;

        public ListUsersHandler(IUserRepository users)
        {
            _users = users;
        }

        public async Task<IReadOnlyList<UserDto>> HandleAsync(ListUsersQuery query)
        {
            var role = string.IsNullOrWhiteSpace(query.Role) ? null : query.Role.Trim();
            var email = string.IsNullOrWhiteSpace(query.Email) ? null : query.Email.Trim();
            var name = string.IsNullOrWhiteSpace(query.Name) ? null : query.Name.Trim();

            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

            if (!string.IsNullOrEmpty(role) && !UserRules.IsAllowedRole(role))
                throw new AppException(HttpStatusCodes.BadRequest, "Role inválida (Requester, Agent, Manager).");

            var emailLowerContains = string.IsNullOrEmpty(email) ? null : email.ToLowerInvariant();
            var nameLowerContains = string.IsNullOrEmpty(name) ? null : name.ToLowerInvariant();

            var skip = (page - 1) * pageSize;
            var take = pageSize;

            var items = await _users.ListAsync(role, emailLowerContains, nameLowerContains, skip, take);

            return items
                .OrderBy(u => u.Id)
                .Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role))
                .ToList();
        }
    }
}
