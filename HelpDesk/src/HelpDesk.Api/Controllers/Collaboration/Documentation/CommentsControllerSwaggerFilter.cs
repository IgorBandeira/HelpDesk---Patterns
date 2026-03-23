using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.Collaboration.Documentation
{
    public sealed class CommentsControllerSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var route = context.ApiDescription.RelativePath?.ToLowerInvariant();
            var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant();

            if (route == "api/tickets/{ticketid}/comments" && httpMethod == "POST")
            {
                ConfigureAdd(operation);
                return;
            }

            if (route == "api/tickets/{ticketid}/comments" && httpMethod == "GET")
            {
                ConfigureList(operation);
                return;
            }

            if (route == "api/tickets/{ticketid}/comments/{commentid}" && httpMethod == "GET")
            {
                ConfigureGetById(operation);
                return;
            }

            if (route == "api/tickets/{ticketid}/comments/{commentid}" && httpMethod == "PUT")
            {
                ConfigureReplace(operation);
                return;
            }

            if (route == "api/tickets/{ticketid}/comments/{commentid}" && httpMethod == "DELETE")
            {
                ConfigureDelete(operation);
            }
        }

        private static void ConfigureAdd(OpenApiOperation operation)
        {
            operation.Summary = "Adicionar comentário";
            operation.Description =
                """
                **Caso de uso**  
                Adiciona um comentário a um chamado ativo, seja **Público** (visualização de qualquer pessoa) ou **Interno** (visualização somente das pessoas relacionadas ao chamado).

                **Regras**
                - O chamado deve existir.
                - Não é permitido comentar em chamados **Fechados** ou **Cancelados**.
                - `Message` é obrigatória e aceita até **4000** caracteres.
                - `Visibility` aceita:
                  - `Público`
                  - `Interno`
                - Comentário `Interno` só pode ser criado por:
                  - **Manager**
                  - **Requester** do chamado
                  - **Assignee** do chamado

                **Responses**
                - **201**: Comentário criado
                - **400**: Dados inválidos / chamado inativo
                - **403**: Sem permissão para comentário interno
                - **404**: Chamado não encontrado
                """;

            operation.Responses.Remove("200");

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);
            EnsureHeaderParameter(operation, "userId", "ID do usuário autenticado.", true);
            EnsureJsonRequestBody(operation, "Dados do comentário.", true);

            SetResponse(operation, "201", "Comentário criado");
            SetResponse(operation, "400", "Dados inválidos ou chamado inativo");
            SetResponse(operation, "403", "Sem permissão para criar comentário interno");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureList(OpenApiOperation operation)
        {
            operation.Summary = "Listar comentários";
            operation.Description =
                """
                **Caso de uso**  
                Lista os comentários de um chamado.

                **Comportamento**
                - Usuários participantes e managers visualizam comentários públicos e internos.
                - Demais usuários visualizam apenas comentários públicos.

                **Responses**
                - **200**: Lista de comentários
                - **401**: Usuário inválido ou não informado
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);
            EnsureHeaderParameter(operation, "userId", "ID do usuário autenticado.", true);

            SetResponse(operation, "200", "Lista de comentários");
            SetResponse(operation, "401", "Usuário inválido ou não informado");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureGetById(OpenApiOperation operation)
        {
            operation.Summary = "Detalhar comentário";
            operation.Description =
                """
                **Caso de uso**  
                Recupera um comentário específico de um chamado.

                **Comportamento**
                - Comentários internos ficam ocultos para usuários sem permissão.
                - Nesse caso, a API responde como **404**.

                **Responses**
                - **200**: Comentário encontrado
                - **401**: Usuário inválido ou não informado
                - **404**: Chamado ou comentário não encontrado
                """;

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);
            EnsurePathParameter(operation, "commentId", "ID do comentário.", true);
            EnsureHeaderParameter(operation, "userId", "ID do usuário autenticado.", true);

            SetResponse(operation, "200", "Comentário encontrado");
            SetResponse(operation, "401", "Usuário inválido ou não informado");
            SetResponse(operation, "404", "Chamado ou comentário não encontrado");
        }

        private static void ConfigureReplace(OpenApiOperation operation)
        {
            operation.Summary = "Editar mensagem do comentário";
            operation.Description =
                """
                **Caso de uso**  
                Substitui a mensagem de um comentário existente.

                **Regras**
                - Somente o **autor** pode editar.
                - Não é permitido editar comentários de chamados inativos.
                - A nova mensagem é armazenada com prefixo `editado:`.
                - A nova mensagem deve ser diferente da anterior.

                **Responses**
                - **200**: Comentário atualizado
                - **400**: Mensagem inválida / chamado inativo / sem alteração
                - **403**: Comentário pertence a outro usuário
                - **404**: Chamado ou comentário não encontrado
                """;

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);
            EnsurePathParameter(operation, "commentId", "ID do comentário.", true);
            EnsureHeaderParameter(operation, "userId", "ID do usuário autenticado.", true);
            EnsureJsonRequestBody(operation, "Nova mensagem do comentário.", true);

            SetResponse(operation, "200", "Comentário atualizado");
            SetResponse(operation, "400", "Mensagem inválida, chamado inativo ou sem alteração");
            SetResponse(operation, "403", "Não é possível editar comentário de outra pessoa");
            SetResponse(operation, "404", "Chamado ou comentário não encontrado");
        }

        private static void ConfigureDelete(OpenApiOperation operation)
        {
            operation.Summary = "Excluir comentário";
            operation.Description =
                """
                **Caso de uso**  
                Remove um comentário existente.

                **Regras**
                - Somente o **autor** pode excluir.
                - Não é permitido excluir comentários de tickets inativos.

                **Responses**
                - **200**: Comentário excluído
                - **400**: Chamado inativo
                - **403**: Comentário pertence a outro usuário
                - **404**: Chamado ou comentário não encontrado
                """;

            EnsurePathParameter(operation, "ticketId", "ID do chamado.", true);
            EnsurePathParameter(operation, "commentId", "ID do comentário.", true);
            EnsureHeaderParameter(operation, "userId", "ID do usuário autenticado.", true);

            SetResponse(operation, "200", "Comentário excluído");
            SetResponse(operation, "400", "Não é possível excluir em chamado inativo");
            SetResponse(operation, "403", "Não é possível excluir comentário de outra pessoa");
            SetResponse(operation, "404", "Chamado ou comentário não encontrado");
        }

        private static void EnsureHeaderParameter(OpenApiOperation operation, string name, string description, bool required)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var parameter = operation.Parameters.FirstOrDefault(p => p.Name == name && p.In == ParameterLocation.Header);

            if (parameter is null)
            {
                parameter = new OpenApiParameter
                {
                    Name = name,
                    In = ParameterLocation.Header
                };
                operation.Parameters.Add(parameter);
            }

            parameter.Description = description;
            parameter.Required = required;
            parameter.Schema = new OpenApiSchema
            {
                Type = "integer",
                Format = "int32"
            };
        }

        private static void EnsurePathParameter(OpenApiOperation operation, string name, string description, bool required)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var parameter = operation.Parameters.FirstOrDefault(p => p.Name == name && p.In == ParameterLocation.Path);

            if (parameter is null)
            {
                parameter = new OpenApiParameter
                {
                    Name = name,
                    In = ParameterLocation.Path
                };
                operation.Parameters.Add(parameter);
            }

            parameter.Description = description;
            parameter.Required = required;
            parameter.Schema = new OpenApiSchema
            {
                Type = "integer",
                Format = "int32"
            };
        }

        private static void EnsureJsonRequestBody(OpenApiOperation operation, string description, bool required)
        {
            operation.RequestBody ??= new OpenApiRequestBody();
            operation.RequestBody.Description = description;
            operation.RequestBody.Required = required;
            operation.RequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();

            if (!operation.RequestBody.Content.ContainsKey("application/json"))
                operation.RequestBody.Content["application/json"] = new OpenApiMediaType();
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