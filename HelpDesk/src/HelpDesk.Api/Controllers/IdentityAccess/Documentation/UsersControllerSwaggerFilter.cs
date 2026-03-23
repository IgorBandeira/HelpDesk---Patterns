using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.IdentityAccess.Documentation
{
    public sealed class UsersControllerSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var route = context.ApiDescription.RelativePath?.ToLowerInvariant();
            var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant();

            if (route == "api/users" && httpMethod == "POST")
            {
                ConfigureCreate(operation);
                return;
            }

            if (route == "api/users/{id}" && httpMethod == "GET")
            {
                ConfigureGetById(operation);
                return;
            }

            if (route == "api/users" && httpMethod == "GET")
            {
                ConfigureList(operation);
                return;
            }

            if (route == "api/users/{id}" && httpMethod == "PATCH")
            {
                ConfigurePatch(operation);
                return;
            }

            if (route == "api/users/{id}" && httpMethod == "DELETE")
            {
                ConfigureDelete(operation);
            }
        }

        private static void ConfigureCreate(OpenApiOperation operation)
        {
            operation.Summary = "Criar usuário";
            operation.Description =
                """
        **Caso de uso**  
        Cadastrar um novo usuário com nome, e-mail e role.

        **Regras**
        - Apenas **Manager** pode criar.
        - **Name** e **Email** obrigatórios; e-mail **único**.
        - **Role** deve estar entre: `Requester`, `Agent`, `Manager`.

        **Responses**
        - **201**: Usuário criado (`UserResponse`)
        - **400**: Dados inválidos (nome/e-mail/role)
        - **401**: Usuário autenticador inválido/não informado
        - **403**: Autenticador não é Manager
        - **409**: E-mail já cadastrado
        """;

            operation.Responses.Remove("200");

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Dados para criação do usuário.",
                required: true);

            SetResponse(operation, "201", "Usuário criado");
            SetResponse(operation, "400", "Dados inválidos (nome/e-mail/role)");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Apenas Managers podem inserir usuários");
            SetResponse(operation, "409", "Já existe usuário com esse e-mail");
        }

        private static void ConfigureGetById(OpenApiOperation operation)
        {
            operation.Summary = "Detalhar usuário";
            operation.Description =
                """
                **Caso de uso**  
                Recuperar dados do usuário e seus tickets como **Requester** e **Agent**.

                **Responses**
                - **200**: Usuário encontrado (`UserWithTicketsResponse`)
                - **404**: Usuário não encontrado
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do usuário.",
                required: true);

            SetResponse(operation, "200", "Usuário encontrado");
            SetResponse(operation, "404", "Usuário não encontrado");
        }

        private static void ConfigureList(OpenApiOperation operation)
        {
            operation.Summary = "Listar usuários";
            operation.Description =
                """
                **Caso de uso**  
                Consultar usuários por `role`, `email` e `name`.

                **Parâmetros**
                - `role`: deve ser `Requester`, `Agent` ou `Manager`.
                - `email`: filtro por parte do e-mail.
                - `name`: filtro por parte do nome.
                - `page` / `pageSize`: paginação (mínimo 1; padrão 1/20).

                **Responses**
                - **200**: Lista de `UserResponse` (paginada por `page` e `pageSize`)
                - **400**: Role inválida
                """;

            EnsureQueryParameter(
                operation,
                "role",
                "Filtra por role (Requester/Agent/Manager).",
                required: false);

            EnsureQueryParameter(
                operation,
                "email",
                "Filtro por parte do e-mail.",
                required: false);

            EnsureQueryParameter(
                operation,
                "name",
                "Filtro por parte do nome.",
                required: false);

            EnsureQueryParameter(
                operation,
                "page",
                "Página (mínimo 1).",
                required: false,
                schemaType: "integer");

            EnsureQueryParameter(
                operation,
                "pageSize",
                "Tamanho da página (mínimo 1).",
                required: false,
                schemaType: "integer");

            SetResponse(operation, "200", "Lista de usuários");
            SetResponse(operation, "400", "Role inválida (Requester, Agent, Manager)");
        }

        private static void ConfigurePatch(OpenApiOperation operation)
        {
            operation.Summary = "Atualizar parcialmente um usuário";
            operation.Description =
                """
                **Caso de uso**  
                Alterar nome, e-mail e/ou role.

                **Regras**
                - Apenas **Manager** pode atualizar.
                - Se alterar **Email**, deve ser único.
                - Se alterar **Role**, deve estar entre `Requester`, `Agent`, `Manager`.

                **Responses**
                - **200**: Usuário atualizado (`UserResponse`)
                - **400**: Nenhuma mudança detectada / role inválida / e-mail inválido
                - **401**: Usuário autenticador inválido/não informado
                - **403**: Autenticador não é Manager
                - **404**: Usuário não encontrado
                - **409**: Conflito de e-mail
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do usuário a ser atualizado.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Campos a serem atualizados.",
                required: false);

            SetResponse(operation, "200", "Usuário atualizado");
            SetResponse(operation, "400", "Nenhuma alteração detectada / dados inválidos");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Apenas Managers podem atualizar usuários");
            SetResponse(operation, "404", "Usuário não encontrado");
            SetResponse(operation, "409", "Já existe usuário com esse e-mail");
        }

        private static void ConfigureDelete(OpenApiOperation operation)
        {
            operation.Summary = "Excluir usuário";
            operation.Description =
                """
                **Caso de uso**  
                Remover um usuário que **não** possua vínculos **ativos** em tickets.

                **Regras**
                - Apenas **Manager** pode excluir.
                - Bloqueia exclusão se usuário possuir tickets **ativos**:
                  - Como **Agent** (atribuído)
                  - Como **Requester** (solicitante)

                **Responses**
                - **200**: Usuário excluído com sucesso
                - **401**: Usuário autenticador inválido/não informado
                - **403**: Autenticador não é Manager
                - **404**: Usuário não encontrado
                - **409**: Possui tickets ativos (agent/requester)
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do usuário a excluir.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            SetResponse(operation, "200", "Usuário excluído com sucesso");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Apenas Managers podem excluir usuários");
            SetResponse(operation, "404", "Usuário não encontrado");
            SetResponse(operation, "409", "Usuário possui tickets ativos vinculados");
        }

        private static void EnsureHeaderParameter(
            OpenApiOperation operation,
            string name,
            string description,
            bool required)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var parameter = operation.Parameters.FirstOrDefault(p =>
                p.Name == name && p.In == ParameterLocation.Header);

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
            parameter.Schema = new OpenApiSchema { Type = "integer", Format = "int32" };
        }

        private static void EnsurePathParameter(
            OpenApiOperation operation,
            string name,
            string description,
            bool required)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var parameter = operation.Parameters.FirstOrDefault(p =>
                p.Name == name && p.In == ParameterLocation.Path);

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
            parameter.Schema = new OpenApiSchema { Type = "integer", Format = "int32" };
        }

        private static void EnsureQueryParameter(
            OpenApiOperation operation,
            string name,
            string description,
            bool required,
            string schemaType = "string")
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var parameter = operation.Parameters.FirstOrDefault(p =>
                p.Name == name && p.In == ParameterLocation.Query);

            if (parameter is null)
            {
                parameter = new OpenApiParameter
                {
                    Name = name,
                    In = ParameterLocation.Query
                };
                operation.Parameters.Add(parameter);
            }

            parameter.Description = description;
            parameter.Required = required;
            parameter.Schema = new OpenApiSchema
            {
                Type = schemaType,
                Format = schemaType == "integer" ? "int32" : null
            };
        }

        private static void EnsureJsonRequestBody(
            OpenApiOperation operation,
            string description,
            bool required)
        {
            operation.RequestBody ??= new OpenApiRequestBody();
            operation.RequestBody.Description = description;
            operation.RequestBody.Required = required;
            operation.RequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();

            if (!operation.RequestBody.Content.ContainsKey("application/json"))
            {
                operation.RequestBody.Content["application/json"] = new OpenApiMediaType();
            }
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