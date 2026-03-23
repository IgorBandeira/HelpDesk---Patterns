namespace HelpDesk.Domain.IdentityAccess.Rules
{
    public static class UserRules
    {
        public static readonly string[] AllowedRoles = { "Requester", "Agent", "Manager" };

        public static bool IsAllowedRole(string role)
            => AllowedRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }
}
