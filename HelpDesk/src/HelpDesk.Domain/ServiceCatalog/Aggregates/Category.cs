using HelpDesk.Domain.ServiceCatalog.ValueObjects;
using HelpDesk.Domain.SharedKernel.Primitives;

namespace HelpDesk.Domain.ServiceCatalog.Aggregates;

public sealed class Category : AggregateRoot<int>
{
    public CategoryName Name { get; private set; }
    public int? ParentId { get; private set; }

    private Category() { }

    private Category(int id, CategoryName name, int? parentId) : base(id)
    {
        Name = name;
        ParentId = parentId;
    }

    public static Category CreateNew(CategoryName name, int? parentId)
    {
        return new Category(0, name, parentId);
    }
}
