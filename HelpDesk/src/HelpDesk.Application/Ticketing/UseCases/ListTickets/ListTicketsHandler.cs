using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Application.Ticketing.UseCases.ListTickets;
using HelpDesk.Domain.Ticketing.Enums;

public sealed class ListTicketsHandler
{
    private readonly ITicketQueryPort _tickets;

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        TicketStatus.Novo,
        TicketStatus.EmAnalise,
        TicketStatus.EmAndamento,
        TicketStatus.Resolvido,
        TicketStatus.Fechado,
        TicketStatus.Cancelado
    };

    private static readonly HashSet<string> ValidPriorities = new(StringComparer.OrdinalIgnoreCase)
    {
        TicketPriority.Baixa,
        TicketPriority.Media,
        TicketPriority.Alta,
        TicketPriority.Critica
    };

    public ListTicketsHandler(ITicketQueryPort tickets) => _tickets = tickets;

    public async Task<IReadOnlyList<TicketListItemDto>> HandleAsync(ListTicketsQuery q)
    {
        if (!string.IsNullOrWhiteSpace(q.Status) && !ValidStatuses.Contains(q.Status))
            throw new AppException(HttpStatusCodes.BadRequest, "Status inválido.");

        if (!string.IsNullOrWhiteSpace(q.Priority) && !ValidPriorities.Contains(q.Priority))
            throw new AppException(HttpStatusCodes.BadRequest, "Prioridade inválida.");

        if (q.Page <= 0)
            throw new AppException(HttpStatusCodes.BadRequest, "Página inválida.");

        if (q.PageSize <= 0)
            throw new AppException(HttpStatusCodes.BadRequest, "Tamanho da página inválido.");

        return await _tickets.ListAsync(
            q.Status, q.Priority, q.Title,
            q.CreatedFrom, q.CreatedTo,
            q.RequesterId, q.AssigneeId, q.CategoryId,
            q.SlaDueFrom, q.SlaDueTo,
            q.OverdueOnly, q.Page, q.PageSize);
    }
}