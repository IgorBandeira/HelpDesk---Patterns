namespace HelpDesk.Application.IdentityAccess.DTOs
{
    public sealed class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
