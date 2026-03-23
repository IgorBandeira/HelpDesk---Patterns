using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.Attachments.Documentation
{
    public sealed class AttachmentsControllerTagsDocumentFilter : IDocumentFilter
    {
        private const string TagName = "Attachments";

        private const string TagDescription =
            """
            📎 Anexos de chamados - Upload, consulta e exclusão de anexos associados aos chamados
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