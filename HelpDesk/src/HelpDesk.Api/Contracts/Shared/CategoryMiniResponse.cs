namespace HelpDesk.Api.Contracts.Ticketing.Responses
{
    public sealed class CategoryMiniResponse
    {
        public int? Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}