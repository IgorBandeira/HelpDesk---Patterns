using HelpDesk.Application.ServiceCatalog.DTOs;
using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Application.Shared.Errors;

namespace HelpDesk.Application.ServiceCatalog.UseCases.GetCategoryById
{
    public sealed class GetCategoryByIdHandler
    {
        private readonly ICategoryRepository _categories;

        public GetCategoryByIdHandler(ICategoryRepository categories)
        {
            _categories = categories;
        }

        public async Task<CategoryItemDto> HandleAsync(GetCategoryByIdQuery query)
        {
            var items = await _categories.ListAsync(
                nameContains: null,
                parentId: null,
                skip: 0,
                take: int.MaxValue);

            var item = items.FirstOrDefault(x => x.Id == query.Id);
            if (item is null)
                throw new AppException(HttpStatusCodes.NotFound, "Categoria não encontrada.");

            var displayName = item.ParentName is not null ? $"{item.ParentName} - {item.Name}" : item.Name;
            return new CategoryItemDto(item.Id, displayName, item.ParentId);
        }
    }
}
