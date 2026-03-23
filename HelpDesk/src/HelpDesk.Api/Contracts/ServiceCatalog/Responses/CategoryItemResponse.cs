namespace HelpDesk.Api.Contracts.ServiceCatalog.Responses
{
    public sealed class CategoryItemResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int? ParentId { get; init; }
    }
}
