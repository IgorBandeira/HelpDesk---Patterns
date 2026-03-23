namespace HelpDesk.Api.Contracts.IdentityAccess.Responses
{
    public sealed record UserResponse(int Id, string Name, string Email, string Role);
}
