using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.ServiceCatalog.Documentation
{
    public sealed class CategoriesControllerSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var route = context.ApiDescription.RelativePath?.ToLowerInvariant();
            var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant();

            if (route == "api/categories" && httpMethod == "POST")
            {
                ConfigureCreate(operation);
                return;
            }

            if (route == "api/categories" && httpMethod == "GET")
            {
                ConfigureList(operation);
                return;
            }

            if (route == "api/categories/{id}" && httpMethod == "GET")
            {
                ConfigureGetById(operation);
                return;
            }

            if (route == "api/categories/{id}" && httpMethod == "DELETE")
            {
                ConfigureDelete(operation);
            }
        }

        private static void ConfigureCreate(OpenApiOperation operation)
        {
            operation.Summary = "Criar categoria";
            operation.Description =
                """
                **Caso de uso**  
                Cadastrar uma nova categoria para classificar tickets.

                **Regras**
                - Apenas **Manager** pode criar.
                - **Name** é obrigatório, com **trim** e tamanho máximo de **180** caracteres.
                - **Name** deve ser **único**.
                - Se **ParentId** for informado:
                  - a categoria pai deve existir;
                  - a categoria pai não pode ser uma subcategoria.
                - A hierarquia suporta no máximo **dois níveis**.

                **Responses**
                - **201**: Categoria criada (`CategoryItemResponse`)
                - **400**: Nome inválido / pai inexistente
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Apenas Managers podem criar categorias
                - **409**: Nome já existente / nível hierárquico inválido
                """;

            operation.Responses.Remove("200");

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Dados para criação da categoria.",
                required: true);

            SetResponse(operation, "201", "Categoria criada");
            SetResponse(operation, "400", "Nome inválido ou categoria pai inexistente");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Apenas Managers podem criar categorias");
            SetResponse(operation, "409", "Já existe categoria com esse nome ou hierarquia inválida");
        }

        private static void ConfigureList(OpenApiOperation operation)
        {
            operation.Summary = "Listar categorias";
            operation.Description =
                """
                **Caso de uso**  
                Consultar categorias por nome e/ou por pai, com paginação.

                **Parâmetros**
                - `nameContains`: filtro por parte do nome.
                - `parentId`: filtra categorias cujo pai é o valor informado.
                - `page` / `pageSize`: paginação (mínimo 1; padrão 1/20).

                **Ordenação**
                - Ordena por `ParentId`, depois por `Name`.

                **Responses**
                - **200**: Lista de `CategoryItemResponse`
                - **400**: Parâmetros inválidos
                """;

            EnsureQueryParameter(
                operation,
                "nameContains",
                "Filtro por parte do nome.",
                required: false);

            EnsureQueryParameter(
                operation,
                "parentId",
                "Filtra por ID do pai.",
                required: false,
                schemaType: "integer");

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

            SetResponse(operation, "200", "Lista de categorias");
            SetResponse(operation, "400", "Parâmetros inválidos");
        }

        private static void ConfigureGetById(OpenApiOperation operation)
        {
            operation.Summary = "Detalhar categoria";
            operation.Description =
                """
                **Caso de uso**  
                Recuperar uma categoria específica pelo ID.

                **Comportamento**
                - Quando houver categoria pai, o será exibido no formato `Pai - Nome`.

                **Responses**
                - **200**: Categoria encontrada (`CategoryItemResponse`)
                - **404**: Categoria não encontrada
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID da categoria.",
                required: true);

            SetResponse(operation, "200", "Categoria encontrada");
            SetResponse(operation, "404", "Categoria não encontrada");
        }

        private static void ConfigureDelete(OpenApiOperation operation)
        {
            operation.Summary = "Excluir categoria";
            operation.Description =
                """
                **Caso de uso**  
                Remover uma categoria sem filhos e sem chamados ativos associados.

                **Regras**
                - Apenas **Manager** pode excluir.
                - A categoria não pode possuir **subcategorias**.
                - A categoria não pode estar associada a **chamados ativos**.
                - Considera ticket ativo quando o status é diferente de **Fechado** e **Cancelado**.

                **Responses**
                - **200**: Categoria excluída com sucesso
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Apenas Managers podem excluir categorias
                - **404**: Categoria não encontrada
                - **409**: Categoria possui subcategorias ou chamados ativos associados
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID da categoria a excluir.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            SetResponse(operation, "200", "Categoria excluída com sucesso");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Apenas Managers podem excluir categorias");
            SetResponse(operation, "404", "Categoria não encontrada");
            SetResponse(operation, "409", "Categoria possui subcategorias ou chamados ativos associados");
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
            parameter.Schema = new OpenApiSchema
            {
                Type = "integer",
                Format = "int32"
            };
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
            parameter.Schema = new OpenApiSchema
            {
                Type = "integer",
                Format = "int32"
            };
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
