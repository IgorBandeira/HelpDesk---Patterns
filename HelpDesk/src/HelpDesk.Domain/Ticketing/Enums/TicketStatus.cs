using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Domain.Ticketing.Enums
{
    public static class TicketStatus
    {
        public const string Novo = "Novo";
        public const string EmAnalise = "Em Análise";
        public const string EmAndamento = "Em Andamento";
        public const string Resolvido = "Resolvido";
        public const string Fechado = "Fechado";
        public const string Cancelado = "Cancelado";

        public static readonly string[] All =
        {
            Novo, EmAnalise, EmAndamento, Resolvido, Fechado, Cancelado
        };

        public static void EnsureValid(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new DomainException("O status é obrigatório.");

            if (!All.Contains(status))
                throw new DomainException($"Status inválido: '{status}'.");
        }
    }
}