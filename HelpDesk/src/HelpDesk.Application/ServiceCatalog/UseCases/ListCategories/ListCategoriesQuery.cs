namespace HelpDesk.Application.ServiceCatalog.UseCases.ListCategories
{
    public sealed record ListCategoriesQuery(
        string? NameContains,
        int? ParentId,
        int Page = 1,
        int PageSize = 20
    );
}
