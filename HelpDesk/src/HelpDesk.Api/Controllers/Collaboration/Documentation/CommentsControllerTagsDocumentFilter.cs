using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.Collaboration.Documentation
{
    public sealed class CommentsControllerTagsDocumentFilter : IDocumentFilter
    {
        private const string TagName = "Comments";
        private const string TagDescription =
            """
            💬 Comentários de chamados - comentários públicos e internos vinculados aos chamados
            """;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags ??= new List<OpenApiTag>();

            var tag = swaggerDoc.Tags.FirstOrDefault(t => t.Name == TagName);

            if (tag is null)
            {
                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = TagName,
                    Description = TagDescription
                });
                return;
            }

            tag.Description = TagDescription;
        }
    }
}