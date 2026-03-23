namespace HelpDesk.Api.Contracts.IdentityAccess.Requests
{
    public sealed class UpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
