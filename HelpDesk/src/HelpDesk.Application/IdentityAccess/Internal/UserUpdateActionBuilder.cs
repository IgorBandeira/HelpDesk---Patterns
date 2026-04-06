using System;
using System.Collections.Generic;

namespace HelpDesk.Application.IdentityAccess.Internal
{
    internal static class UserUpdateActionBuilder
    {
        internal static IReadOnlyList<string> Build(
            string actorUserName,
            string previousName,
            string currentName,
            string previousEmail,
            string currentEmail,
            string previousRole,
            string currentRole)
        {
            var actions = new List<string>();

            if (!string.Equals(previousName, currentName, StringComparison.Ordinal))
            {
                actions.Add($"Nome do usuário alterado de '{previousName}' para '{currentName}' por {actorUserName}");
            }

            if (!string.Equals(previousEmail, currentEmail, StringComparison.OrdinalIgnoreCase))
            {
                actions.Add($"E-mail do usuário alterado de '{previousEmail}' para '{currentEmail}' por {actorUserName}");
            }

            if (!string.Equals(previousRole, currentRole, StringComparison.OrdinalIgnoreCase))
            {
                actions.Add($"Papel do usuário alterado de '{previousRole}' para '{currentRole}' por {actorUserName}");
            }

            return actions;
        }
    }
}