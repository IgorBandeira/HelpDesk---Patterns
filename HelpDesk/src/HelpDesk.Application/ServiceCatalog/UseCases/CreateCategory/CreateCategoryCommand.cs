using HelpDesk.Application.ServiceCatalog.DTOs;

namespace HelpDesk.Application.ServiceCatalog.UseCases.CreateCategory
{
    public sealed record CreateCategoryCommand(int UserId, CreateCategoryDto Dto);

}
