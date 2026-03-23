using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Ticketing.Enums
{
    public static class TicketPriority
    {
        public const string Baixa = "Baixa";
        public const string Media = "Média";
        public const string Alta = "Alta";
        public const string Critica = "Crítica";

        public static readonly string[] All = { Baixa, Media, Alta, Critica };

        public static void EnsureValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Prioridade é obrigatória.");

            if (!All.Contains(value.Trim(), StringComparer.Ordinal))
                throw new DomainException("Prioridade inválida (Baixa, Média, Alta, Crítica).");
        }

        public static TimeSpan ToSla(string priority)
        {
            return priority switch
            {
                Baixa => TimeSpan.FromHours(72),
                Media => TimeSpan.FromHours(48),
                Alta => TimeSpan.FromHours(24),
                Critica => TimeSpan.FromHours(8),
                _ => throw new DomainException("Prioridade inválida (Baixa, Média, Alta, Crítica).")
            };
        }
    }
}
