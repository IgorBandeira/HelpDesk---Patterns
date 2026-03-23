namespace HelpDesk.Api.Contracts.Ticketing.Requests
{
    public sealed class CreateTicketRequest
    {
        public string Title { get; init; } = "";
        public string Description { get; init; } = "";
        public string Priority { get; init; } = "Média";
        public int CategoryId { get; init; }
    }
}