namespace HelpDesk.Api.Contracts.Ticketing.Requests
{
    public sealed class UpdateTicketRequest
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public string? Priority { get; init; }
        public int? CategoryId { get; init; }
    }
}