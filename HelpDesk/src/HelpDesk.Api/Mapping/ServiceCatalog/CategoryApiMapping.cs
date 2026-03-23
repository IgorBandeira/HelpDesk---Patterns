using HelpDesk.Api.Contracts.ServiceCatalog.Requests;
using HelpDesk.Api.Contracts.ServiceCatalog.Responses;
using HelpDesk.Application.ServiceCatalog.DTOs;

namespace HelpDesk.Api.Mapping.ServiceCatalog
{
    public static class CategoryApiMapping
    {
        public static CreateCategoryDto ToApplicationDto(this CreateCategoryRequest request)
        {
            return new CreateCategoryDto
            {
                Name = request.Name,
                ParentId = request.ParentId
            };
        }

        public static CategoryItemResponse ToResponse(this CategoryItemDto dto)
        {
            return new CategoryItemResponse
            {
                Id = dto.Id,
                Name = dto.Name,
                ParentId = dto.ParentId
            };
        }

        public static IReadOnlyList<CategoryItemResponse> ToResponseList(
            this IReadOnlyList<CategoryItemDto> items)
        {
            return items.Select(x => x.ToResponse()).ToList();
        }
    }
}
