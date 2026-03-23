using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.Attachments.Documentation
{
    public sealed class AttachmentsControllerSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var route = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
            var method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;

            if (route.Contains("api/tickets/") && route.Contains("/attachments") && !route.Contains("{attachmentid}") && method == "POST")
            {
                ConfigureUpload(operation);
                return;
            }

            if (route.Contains("api/tickets/") && route.Contains("/attachments") && !route.Contains("{attachmentid}") && method == "GET")
            {
                ConfigureList(operation);
                return;
            }

            if (route.Contains("api/tickets/") && route.Contains("/attachments/") && method == "GET")
            {
                ConfigureGetById(operation);
                return;
            }

            if (route.Contains("api/tickets/") && route.Contains("/attachments/") && method == "DELETE")
            {
                ConfigureDelete(operation);
            }
        }

        private static void ConfigureUpload(OpenApiOperation operation)
        {
            operation.Summary = "Anexar arquivo ao chamado";
            operation.Description =
                """
                **Caso de uso**  
                Registrar um anexo em um chamado e persistir o arquivo no storage.

                **Regras**
                - Não é permitido anexar em chamados **Fechado** ou **Cancelado**.
                - O arquivo é enviado via **multipart/form-data**.
                - Tamanho máximo: **10MB**.
                - Extensões bloqueadas: `.exe`, `.bat`, `.sh`.

                **Responses**
                - **201**: Anexo criado com sucesso
                - **400**: Arquivo inválido / chamado inativo / usuário inválido
                - **401**: Usuário não informado
                - **404**: Chamado não encontrado
                """;

            operation.Responses.Remove("200");

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);
            EnsureHeaderParameter(operation, "userId", "ID do usuário autenticador.", true);
            EnsureMultipartRequestBody(operation, "Arquivo do anexo.", true);

            SetResponse(operation, "201", "Anexo criado com sucesso");
            SetResponse(operation, "400", "Arquivo inválido, chamado inativo ou usuário inválido");
            SetResponse(operation, "401", "Usuário não informado");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureList(OpenApiOperation operation)
        {
            operation.Summary = "Listar anexos do chamado";
            operation.Description =
                """
                **Caso de uso**  
                Recuperar os anexos vinculados a um chamado.

                **Responses**
                - **200**: Lista de anexos
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);

            SetResponse(operation, "200", "Lista de anexos");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureGetById(OpenApiOperation operation)
        {
            operation.Summary = "Detalhar anexo";
            operation.Description =
                """
                **Caso de uso**  
                Recuperar um anexo específico de um chamado.

                **Responses**
                - **200**: Anexo encontrado
                - **404**: Anexo não encontrado
                """;

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);
            EnsurePathParameter(operation, "attachmentId", "ID do anexo.", true);

            SetResponse(operation, "200", "Anexo encontrado");
            SetResponse(operation, "404", "Anexo não encontrado");
        }

        private static void ConfigureDelete(OpenApiOperation operation)
        {
            operation.Summary = "Excluir anexo";
            operation.Description =
                """
                **Caso de uso**  
                Remove um anexo de um chamado.

                **Regras**
                - Não é permitido excluir anexos de chamados **Fechado** ou **Cancelado**.
                - Apenas o autor do upload pode excluir.

                **Responses**
                - **200**: Anexo excluído com sucesso
                - **400**: Chamado inativo
                - **403**: Usuário não pode excluir anexo de outra pessoa
                - **404**: Chamado ou anexo não encontrado
                """;

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);
            EnsurePathParameter(operation, "attachmentId", "ID do anexo.", true);
            EnsureHeaderParameter(operation, "userId", "ID do usuário autenticador.", true);

            SetResponse(operation, "200", "Anexo excluído com sucesso");
            SetResponse(operation, "400", "Chamado inativo");
            SetResponse(operation, "403", "Usuário não pode excluir anexo de outra pessoa");
            SetResponse(operation, "404", "Chamado ou anexo não encontrado");
        }

        private static void EnsureHeaderParameter(OpenApiOperation operation, string name, string description, bool required)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var parameter = operation.Parameters.FirstOrDefault(p => p.Name == name && p.In == ParameterLocation.Header);
            if (parameter is null)
            {
                parameter = new OpenApiParameter { Name = name, In = ParameterLocation.Header };
                operation.Parameters.Add(parameter);
            }

            parameter.Description = description;
            parameter.Required = required;
            parameter.Schema = new OpenApiSchema { Type = "integer", Format = "int32" };
        }

        private static void EnsurePathParameter(OpenApiOperation operation, string name, string description, bool required)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var parameter = operation.Parameters.FirstOrDefault(p => p.Name == name && p.In == ParameterLocation.Path);
            if (parameter is null)
            {
                parameter = new OpenApiParameter { Name = name, In = ParameterLocation.Path };
                operation.Parameters.Add(parameter);
            }

            parameter.Description = description;
            parameter.Required = required;
            parameter.Schema = new OpenApiSchema { Type = "integer", Format = "int32" };
        }

        private static void EnsureMultipartRequestBody(OpenApiOperation operation, string description, bool required)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Description = description,
                Required = required,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Required = new HashSet<string> { "file" },
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "Arquivo a ser anexado."
                                }
                            }
                        }
                    }
                }
            };
        }

        private static void SetResponse(OpenApiOperation operation, string statusCode, string description)
        {
            operation.Responses ??= new OpenApiResponses();

            if (operation.Responses.ContainsKey(statusCode))
            {
                operation.Responses[statusCode].Description = description;
                return;
            }

            operation.Responses.Add(statusCode, new OpenApiResponse
            {
                Description = description
            });
        }
    }
}