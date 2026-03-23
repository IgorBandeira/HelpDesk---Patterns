using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.Ticketing.Documentation
{
    public sealed class TicketsControllerTagsDocumentFilter : IDocumentFilter
    {
        private const string TicketsTagName = "Tickets";

        private const string TicketsTagDescription =
            """
            🎫 Chamados - Ciclo de vida completo dos chamados, incluindo criação, edição, atribuição, troca de requester, mudança de status, reabertura e cancelamento
            """;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags ??= new List<OpenApiTag>();

            var tag = swaggerDoc.Tags.FirstOrDefault(t => t.Name == TicketsTagName);

            if (tag is null)
            {
                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = TicketsTagName,
                    Description = TicketsTagDescription
                });
                return;
            }

            tag.Description = TicketsTagDescription;
        }
    }
}