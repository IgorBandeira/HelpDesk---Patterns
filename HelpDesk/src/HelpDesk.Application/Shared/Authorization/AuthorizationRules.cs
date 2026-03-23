using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Errors;

namespace HelpDesk.Application.Shared.Authorization
{
    public static class AuthorizationRules
    {
        public static void EnsureManager(UserSnapshot? user, string message)
        {
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            if (!string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                throw new AppException(HttpStatusCodes.Forbidden, message);
        }
    }
}