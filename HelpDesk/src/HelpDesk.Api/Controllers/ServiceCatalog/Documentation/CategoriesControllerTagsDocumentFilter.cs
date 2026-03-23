using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.ServiceCatalog.Documentation
{
    public sealed class CategoriesControllerTagsDocumentFilter : IDocumentFilter
    {
        private const string CategoriesTagName = "Categories";

        private const string CategoriesTagDescription =
            """
            🗂️ Categorias de chamados - Organização hierárquica de categorias e subcategorias
            """;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags ??= new List<OpenApiTag>();

            var tag = swaggerDoc.Tags.FirstOrDefault(t => t.Name == CategoriesTagName);

            if (tag is null)
            {
                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = CategoriesTagName,
                    Description = CategoriesTagDescription
                });
                return;
            }

            tag.Description = CategoriesTagDescription;
        }
    }
}
