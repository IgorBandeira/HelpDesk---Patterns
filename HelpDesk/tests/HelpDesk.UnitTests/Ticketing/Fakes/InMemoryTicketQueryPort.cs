using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.UnitTests.Ticketing.Fakes
{
    public sealed class InMemoryTicketQueryPort : ITicketQueryPort
    {
        private readonly List<TicketDetailsDto> _details = new();
        private readonly List<TicketListItemDto> _items = new();

        public Task<TicketDetailsDto?> GetDetailsByIdAsync(int ticketId)
            => Task.FromResult(_details.FirstOrDefault(x => x.Id == ticketId));

        public Task<IReadOnlyList<TicketListItemDto>> ListAsync(
            string? status,
            string? priority,
            string? title,
            DateTime? createdFrom,
            DateTime? createdTo,
            int? requesterId,
            int? assigneeId,
            int? categoryId,
            DateTime? slaDueFrom,
            DateTime? slaDueTo,
            bool? overdueOnly,
            int page,
            int pageSize)
        {
            IEnumerable<TicketListItemDto> query = _items;

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status);
            else
                query = query.Where(x => x.Status != "Cancelado");

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(x => x.Priority == priority);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(x => x.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (createdFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(x => x.CreatedAt <= createdTo.Value);

            if (requesterId.HasValue)
                query = query.Where(x => x.Requester.Id == requesterId.Value);

            if (assigneeId.HasValue)
                query = query.Where(x => x.Assignee != null && x.Assignee.Id == assigneeId.Value);

            if (categoryId.HasValue)
                query = query.Where(x => x.Category.Id == categoryId.Value);

            if (slaDueFrom.HasValue)
                query = query.Where(x => x.SlaDueAt.HasValue && x.SlaDueAt.Value >= slaDueFrom.Value);

            if (slaDueTo.HasValue)
                query = query.Where(x => x.SlaDueAt.HasValue && x.SlaDueAt.Value <= slaDueTo.Value);

            if (overdueOnly == true)
            {
                var now = DateTime.UtcNow;
                query = query.Where(x =>
                    x.SlaDueAt.HasValue &&
                    x.SlaDueAt.Value < now &&
                    x.Status != "Fechado" &&
                    x.Status != "Cancelado");
            }

            var skip = (page - 1) * pageSize;
            var result = query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return Task.FromResult((IReadOnlyList<TicketListItemDto>)result);
        }

        public void SeedList(params TicketListItemDto[] items) => _items.AddRange(items);
        public void SeedDetails(params TicketDetailsDto[] items) => _details.AddRange(items);
    }
}