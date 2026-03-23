using HelpDesk.Application.ServiceCatalog.UseCases.CreateCategory;
using HelpDesk.Application.ServiceCatalog.UseCases.DeleteCategory;
using HelpDesk.Application.ServiceCatalog.UseCases.GetCategoryById;
using HelpDesk.Application.ServiceCatalog.UseCases.ListCategories;

namespace HelpDesk.Api.DependencyInjection
{
    public static class ServiceCatalogModule
    {
        public static IServiceCollection AddServiceCatalogApi(this IServiceCollection services)
        {
            services.AddScoped<CreateCategoryHandler>();
            services.AddScoped<DeleteCategoryHandler>();
            services.AddScoped<GetCategoryByIdHandler>();
            services.AddScoped<ListCategoriesHandler>();

            return services;
        }
    }
}