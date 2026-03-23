using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HelpDesk.Api.Controllers.Ticketing.Documentation
{
    public sealed class TicketsControllerSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var route = context.ApiDescription.RelativePath?.ToLowerInvariant();
            var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant();

            if (route == "api/tickets" && httpMethod == "POST")
            {
                ConfigureCreate(operation);
                return;
            }

            if (route == "api/tickets" && httpMethod == "GET")
            {
                ConfigureList(operation);
                return;
            }

            if (route == "api/tickets/{id}" && httpMethod == "GET")
            {
                ConfigureGetById(operation);
                return;
            }

            if (route == "api/tickets/{id}" && httpMethod == "PATCH")
            {
                ConfigureUpdate(operation);
                return;
            }

            if (route == "api/tickets/{id}/assign" && httpMethod == "PATCH")
            {
                ConfigureAssign(operation);
                return;
            }

            if (route == "api/tickets/{id}/requester" && httpMethod == "PATCH")
            {
                ConfigureChangeRequester(operation);
                return;
            }

            if (route == "api/tickets/{id}/status" && httpMethod == "PATCH")
            {
                ConfigureChangeStatus(operation);
                return;
            }

            if (route == "api/tickets/{id}/reopen" && httpMethod == "PATCH")
            {
                ConfigureReopen(operation);
                return;
            }

            if (route == "api/tickets/{id}/cancel" && httpMethod == "PATCH")
            {
                ConfigureCancel(operation);
            }
        }

        private static void ConfigureCreate(OpenApiOperation operation)
        {
            operation.Summary = "Criar chamado";
            operation.Description =
                """
                **Caso de uso**  
                Abrir um novo chamado com título, descrição, prioridade e categoria.

                **Regras**
                - Apenas **Requester** ou **Manager** podem criar.
                - **Title** é obrigatório, com **trim** e tamanho máximo de **180** caracteres.
                - **Description** é obrigatória.
                - **Priority** deve ser válida.
                - **CategoryId** deve existir.
                - O SLA é calculado com base na prioridade.

                **Responses**
                - **201**: Chamado criado (`TicketResponse`)
                - **400**: Dados inválidos
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Apenas Requester ou Manager podem criar chamados
                """;

            operation.Responses.Remove("200");

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Dados para criação do chamado.",
                required: true);

            SetResponse(operation, "201", "Chamado criado");
            SetResponse(operation, "400", "Dados inválidos");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Apenas Requester ou Manager podem criar chamados");
        }

        private static void ConfigureList(OpenApiOperation operation)
        {
            operation.Summary = "Listar chamados";
            operation.Description =
                """
                **Caso de uso**  
                Consultar chamados por múltiplos filtros com paginação.

                **Parâmetros**
                - `status`: filtra pelo status do chamado.
                - `priority`: filtra pela prioridade.
                - `title`: busca por parte do título.
                - `createdFrom` / `createdTo`: intervalo de criação.
                - `requesterId` / `assigneeId` / `categoryId`.
                - `slaDueFrom` / `slaDueTo`: intervalo do vencimento do SLA.
                - `overdueOnly`: retorna apenas chamados vencidos.
                - `page` / `pageSize`: paginação (mínimo 1; padrão 1/20).

                **Comportamento**
                - Retorna uma coleção de `TicketListItemResponse`.

                **Responses**
                - **200**: Lista de chamados
                - **400**: Parâmetros inválidos
                """;

            EnsureQueryParameter(operation, "status", "Status do chamado.", false);
            EnsureQueryParameter(operation, "priority", "Prioridade do chamado.", false);
            EnsureQueryParameter(operation, "title", "Busca por parte do título.", false);
            EnsureQueryParameter(operation, "createdFrom", "Criado a partir de (>=).", false, "string", "date-time");
            EnsureQueryParameter(operation, "createdTo", "Criado até (<=).", false, "string", "date-time");
            EnsureQueryParameter(operation, "requesterId", "Filtra por ID do solicitante.", false, "integer", "int32");
            EnsureQueryParameter(operation, "assigneeId", "Filtra por ID do responsável.", false, "integer", "int32");
            EnsureQueryParameter(operation, "categoryId", "Filtra por ID da categoria.", false, "integer", "int32");
            EnsureQueryParameter(operation, "slaDueFrom", "SLA vencendo a partir de (>=).", false, "string", "date-time");
            EnsureQueryParameter(operation, "slaDueTo", "SLA vencendo até (<=).", false, "string", "date-time");
            EnsureQueryParameter(operation, "overdueOnly", "Apenas chamados vencidos.", false, "boolean");
            EnsureQueryParameter(operation, "page", "Página (mínimo 1).", false, "integer", "int32");
            EnsureQueryParameter(operation, "pageSize", "Tamanho da página (mínimo 1).", false, "integer", "int32");

            SetResponse(operation, "200", "Lista de chamados");
            SetResponse(operation, "400", "Parâmetros inválidos");
        }

        private static void ConfigureGetById(OpenApiOperation operation)
        {
            operation.Summary = "Detalhar chamado";
            operation.Description =
                """
                **Caso de uso**  
                Recuperar um chamado específico pelo ID.

                **Regras**
                - Requer header **userId**.
                - Retorna os detalhes completos do chamado.

                **Responses**
                - **200**: Chamado encontrado (`TicketDetailsResponse`)
                - **401**: Usuário autenticador inválido ou não informado
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do chamado.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            SetResponse(operation, "200", "Chamado encontrado");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureUpdate(OpenApiOperation operation)
        {
            operation.Summary = "Editar chamado";
            operation.Description =
                """
                **Caso de uso**  
                Atualizar parcialmente um chamado. Editar título, descrição, prioridade e/ou categoria.

                **Regras**
                - Somente **Owner** (Requester original) ou **Manager**.
                - Ticket **não** pode estar **Fechado/Cancelado**.
                - Se mudar **Priority**, reinicia `SlaStartAt` e recalcula `SlaDueAt`.
                - Cada alteração gera uma **TicketAction** e **notificação**.
                - Retorna **400** se campos inválidos ou sem mudanças.

                **Responses**
                - **200**: Chamado atualizado (`TicketResponse`)
                - **400**: Dados inválidos / nenhuma alteração detectada / chamado inativo
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Usuário sem permissão para editar
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do chamado.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Campos a serem atualizados.",
                required: true);

            SetResponse(operation, "200", "Chamado atualizado");
            SetResponse(operation, "400", "Dados inválidos, nenhuma alteração detectada ou chamado inativo");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Usuário sem permissão para editar");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureAssign(OpenApiOperation operation)
        {
            operation.Summary = "Atribuir chamado";
            operation.Description =
                """
                **Caso de uso**  
                Definir o responsável técnico do chamado.

                **Regras**
                - Chamado deve estar ativo.
                - O usuário atribuído deve existir e ser um **Agent**.
                - A operação segue as permissões definidas no caso de uso.

                **Responses**
                - **200**: Chamado atribuído (`AssignTicketResponse`)
                - **400**: Chamado inativo ou agent inválido
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Usuário sem permissão para atribuir
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do chamado.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Dados para atribuição do chamado.",
                required: true);

            SetResponse(operation, "200", "Chamado atribuído");
            SetResponse(operation, "400", "Chamado inativo ou agent inválido");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Usuário sem permissão para atribuir");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureChangeRequester(OpenApiOperation operation)
        {
            operation.Summary = "Trocar requester";
            operation.Description =
                """
                **Caso de uso**  
                Alterar o solicitante do chamado.

                **Regras**
                - Chamado deve estar ativo.
                - O novo usuário deve existir e ser um **Requester**.
                - A operação segue as permissões definidas no caso de uso.

                **Responses**
                - **200**: Requester alterado (`RequesterResponse`)
                - **400**: Chamado inativo ou requester inválido
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Usuário sem permissão para alterar requester
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do chamado.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Dados para troca de requester.",
                required: true);

            SetResponse(operation, "200", "Requester alterado");
            SetResponse(operation, "400", "Chamado inativo ou requester inválido");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Usuário sem permissão para alterar requester");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureChangeStatus(OpenApiOperation operation)
        {
            operation.Summary = "Alterar status";
            operation.Description =
                """
                **Caso de uso**  
                Realizar transições válidas do workflow do chamado.

                **Transições permitidas**
                - `Em Análise -> Em Andamento`
                - `Em Andamento -> Resolvido`
                - `Resolvido -> Fechado`

                **Regras**
                - Requer header **userId**.
                - Algumas transições exigem que o usuário logado seja o **Agent** atribuído.
                - O fechamento exige que o usuário logado seja o **Requester** do chamado.

                **Responses**
                - **200**: Status alterado (`ChangeStatusResponse`)
                - **400**: Transição inválida ou pré-condições não atendidas
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Usuário sem permissão para a transição
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do chamado.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Nova situação do chamado.",
                required: true);

            SetResponse(operation, "200", "Status alterado");
            SetResponse(operation, "400", "Transição inválida ou pré-condições não atendidas");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Usuário sem permissão para a transição");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureReopen(OpenApiOperation operation)
        {
            operation.Summary = "Reabrir chamado";
            operation.Description =
                """
                **Caso de uso**  
                Reabrir um chamado encerrado para nova análise.

                **Regras**
                - Requer header **userId**.
                - Apenas chamados em **Resolvido** ou **Fechado** podem ser reabertos.
                - O motivo da reabertura é obrigatório.
                - A operação reinicia o SLA conforme a regra de negócio.

                **Responses**
                - **200**: Chamado reaberto (`ReopenTicketResponse`)
                - **400**: Status atual inválido ou motivo não informado
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Usuário sem permissão para reabrir
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do chamado.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Motivo da reabertura.",
                required: true);

            SetResponse(operation, "200", "Chamado reaberto");
            SetResponse(operation, "400", "Status atual inválido ou motivo não informado");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Usuário sem permissão para reabrir");
            SetResponse(operation, "404", "Chamado não encontrado");
        }

        private static void ConfigureCancel(OpenApiOperation operation)
        {
            operation.Summary = "Cancelar chamado";
            operation.Description =
                """
                **Caso de uso**  
                Cancelar um chamado em fase inicial.

                **Regras**
                - Requer header **userId**.
                - Só pode cancelar chamados em **Novo** ou **Em Análise**.
                - O motivo do cancelamento é obrigatório.
                - A operação segue as permissões definidas no caso de uso.

                **Responses**
                - **200**: Chamado cancelado (`CancelTicketResponse`)
                - **400**: Status atual inválido ou motivo não informado
                - **401**: Usuário autenticador inválido ou não informado
                - **403**: Usuário sem permissão para cancelar
                - **404**: Chamado não encontrado
                """;

            EnsurePathParameter(
                operation,
                "id",
                "ID do chamado.",
                required: true);

            EnsureHeaderParameter(
                operation,
                "userId",
                "ID do usuário autenticador (header obrigatório).",
                required: true);

            EnsureJsonRequestBody(
                operation,
                "Motivo do cancelamento.",
                required: true);

            SetResponse(operation, "200", "Chamado cancelado");
            SetResponse(operation, "400", "Status atual inválido ou motivo não informado");
            SetResponse(operation, "401", "Usuário autenticador inválido ou não informado");
            SetResponse(operation, "403", "Usuário sem permissão para cancelar");
            SetResponse(operation, "404", "Chamado não encontrado");
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
            string schemaType = "string",
            string? format = null)
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
                Format = format
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