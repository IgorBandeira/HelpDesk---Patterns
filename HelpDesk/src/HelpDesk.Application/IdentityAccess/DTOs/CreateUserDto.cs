namespace HelpDesk.Application.IdentityAccess.DTOs
{
    public sealed class CreateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; } = "Requester";
    }
}
