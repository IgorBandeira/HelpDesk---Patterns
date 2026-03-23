namespace HelpDesk.Api.Contracts.ServiceCatalog.Requests
{
    public sealed class CreateCategoryRequest
    {
        public string? Name { get; set; }
        public int? ParentId { get; set; }
    }
}
