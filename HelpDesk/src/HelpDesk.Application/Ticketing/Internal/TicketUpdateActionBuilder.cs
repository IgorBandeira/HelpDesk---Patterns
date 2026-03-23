namespace HelpDesk.Application.Ticketing.Internal
{
    internal static class TicketUpdateActionBuilder
    {
        internal static IReadOnlyList<string> Build(
            string userName,
            string previousTitle,
            string currentTitle,
            string previousDescription,
            string currentDescription,
            string previousPriority,
            string currentPriority,
            int previousCategoryId,
            int currentCategoryId,
            string? oldCategoryName,
            string? newCategoryName)
        {
            var actions = new List<string>();

            if (!string.Equals(previousTitle, currentTitle, StringComparison.Ordinal))
            {
                actions.Add($"Título do chamado alterado para: '{currentTitle}' - por {userName}");
            }

            if (!string.Equals(previousDescription, currentDescription, StringComparison.Ordinal))
            {
                actions.Add($"Descrição do chamado alterada para: '{currentDescription}' - por {userName}");
            }

            if (!string.Equals(previousPriority, currentPriority, StringComparison.OrdinalIgnoreCase))
            {
                actions.Add($"Prioridade alterada para {currentPriority} por {userName}");
            }

            if (previousCategoryId != currentCategoryId)
            {
                var action = oldCategoryName != null
                    ? $"Categoria '{oldCategoryName}' foi trocada para '{newCategoryName}' por {userName}."
                    : $"Categoria definida como '{newCategoryName}' por {userName}.";

                actions.Add(action);
            }

            return actions;
        }
    }
}