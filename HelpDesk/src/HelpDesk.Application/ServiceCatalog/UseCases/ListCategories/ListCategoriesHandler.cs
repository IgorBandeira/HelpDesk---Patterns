using HelpDesk.Application.ServiceCatalog.DTOs;
using HelpDesk.Application.ServiceCatalog.Ports;

namespace HelpDesk.Application.ServiceCatalog.UseCases.ListCategories
{
    public sealed class ListCategoriesHandler
    {
        private readonly ICategoryRepository _categories;

        public ListCategoriesHandler(ICategoryRepository categories)
        {
            _categories = categories;
        }

        public async Task<IReadOnlyList<CategoryItemDto>> HandleAsync(ListCategoriesQuery query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

            var skip = (page - 1) * pageSize;
            var take = pageSize;

            _ = await _categories.CountAsync(query.NameContains, query.ParentId);

            var items = await _categories.ListAsync(
                query.NameContains,
                query.ParentId,
                skip,
                take);

            return items
                .OrderBy(i => i.ParentId)
                .ThenBy(i => i.Name)
                .Select(i =>
                {
                    var displayName = i.ParentName is not null ? $"{i.ParentName} - {i.Name}" : i.Name;
                    return new CategoryItemDto(i.Id, displayName, i.ParentId);
                })
                .ToList();
        }
    }
}
