namespace HelpDesk.Application.IdentityAccess.UseCases.ListUsers
{
    public sealed record ListUsersQuery(
        string? Role,
        string? Email,
        string? Name,
        int Page = 1,
        int PageSize = 20
    );
}
