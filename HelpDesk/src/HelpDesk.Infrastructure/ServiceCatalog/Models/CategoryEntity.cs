namespace HelpDesk.Infrastructure.ServiceCatalog.Models
{
    public sealed class CategoryEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }

        public CategoryEntity? Parent { get; set; }
        public List<CategoryEntity> Children { get; set; } = new();
    }
}
