namespace HelpDesk.Application.IdentityAccess.DTOs
{
    public sealed record UserDto(int Id, string Name, string Email, string Role);
}
