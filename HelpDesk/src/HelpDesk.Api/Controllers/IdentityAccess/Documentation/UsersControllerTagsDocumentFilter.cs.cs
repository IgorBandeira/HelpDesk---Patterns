using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.IdentityAccess.Documentation
{
    public sealed class UsersControllerTagsDocumentFilter : IDocumentFilter
    {
        private const string UsersTagName = "Users";

        private const string UsersTagDescription =
            """
            👤 Usuários do sistema - Gerentes, Solicitantes e Agentes
            """;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags ??= new List<OpenApiTag>();

            var tag = swaggerDoc.Tags.FirstOrDefault(t => t.Name == UsersTagName);

            if (tag is null)
            {
                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = UsersTagName,
                    Description = UsersTagDescription
                });
                return;
            }

            tag.Description = UsersTagDescription;
        }
    }
}