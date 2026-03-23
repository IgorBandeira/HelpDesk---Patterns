using HelpDesk.Domain.ServiceCatalog.Aggregates;

namespace HelpDesk.Application.ServiceCatalog.Ports;

public interface ICategoryRepository
{
    Task<bool> NameExistsAsync(string name);                 
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByIdNoTrackingAsync(int id);       
    Task<bool> HasChildrenAsync(int id);
    Task AddAsync(Category category);
    Task DeleteAsync(Category category);

    Task<int> CountAsync(string? nameContains, int? parentId);
    Task<IReadOnlyList<CategoryListItem>> ListAsync(
        string? nameContains,
        int? parentId,
        int skip,
        int take);
}

public sealed record CategoryListItem(
    int Id,
    string Name,
    int? ParentId,
    int? ParentCategoryId,
    string? ParentName
);
