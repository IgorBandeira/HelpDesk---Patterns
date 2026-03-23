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
    }

}
