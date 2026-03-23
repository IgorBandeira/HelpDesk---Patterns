namespace HelpDesk.Application.ServiceCatalog.UseCases.DeleteCategory
{
    public sealed record DeleteCategoryCommand(int UserId, int CategoryId);
}
