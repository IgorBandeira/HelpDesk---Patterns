using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Ticketing.Aggregates;

namespace HelpDesk.UnitTests.Ticketing.Fakes
{
    public sealed class InMemoryTicketRepository : ITicketRepository
    {
        private readonly List<Ticket> _tickets = new();
        private int _nextId = 1;

        public Task<Ticket?> GetByIdAsync(int id)
            => Task.FromResult(_tickets.FirstOrDefault(t => t.Id == id));

        public Task AddAsync(Ticket ticket)
        {
            typeof(Ticket).GetProperty("Id")!.SetValue(ticket, _nextId++);
            _tickets.Add(ticket);
            return Task.CompletedTask;
        }

        public Task SaveAsync(Ticket ticket) => Task.CompletedTask;

        public Task<IReadOnlyList<TicketListItemDto>> ListAsync(TicketListFilterDto filter, int skip, int take)
        {
            IEnumerable<TicketListItemDto> q = _tickets.Select(t => new TicketListItemDto(
                t.Id,
                t.Title.Value,
                t.Status,
                t.Priority,
                t.CreatedAt,
                t.SlaDueAt,
                new UserMiniDto(t.RequesterId, $"User {t.RequesterId}"),
                t.AssigneeId.HasValue
                    ? new UserMiniDto(t.AssigneeId.Value, $"User {t.AssigneeId.Value}")
                    : null,
                new CategoryMiniDto(t.CategoryId, $"Category {t.CategoryId}")
            ));

            if (!string.IsNullOrWhiteSpace(filter.Status))
                q = q.Where(x => x.Status == filter.Status);
            else
                q = q.Where(x => x.Status != "Cancelado");

            if (!string.IsNullOrWhiteSpace(filter.Priority))
                q = q.Where(x => x.Priority == filter.Priority);

            if (!string.IsNullOrWhiteSpace(filter.TitleContainsLower))
                q = q.Where(x => x.Title.ToLower().Contains(filter.TitleContainsLower));

            if (filter.CreatedFrom.HasValue)
                q = q.Where(x => x.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                q = q.Where(x => x.CreatedAt <= filter.CreatedTo.Value);

            if (filter.RequesterId.HasValue)
                q = q.Where(x => x.Requester.Id == filter.RequesterId.Value);

            if (filter.AssigneeId.HasValue)
                q = q.Where(x => x.Assignee != null && x.Assignee.Id == filter.AssigneeId.Value);

            if (filter.CategoryId.HasValue)
                q = q.Where(x => x.Category.Id == filter.CategoryId.Value);

            if (filter.SlaDueFrom.HasValue)
                q = q.Where(x => x.SlaDueAt != null && x.SlaDueAt >= filter.SlaDueFrom.Value);

            if (filter.SlaDueTo.HasValue)
                q = q.Where(x => x.SlaDueAt != null && x.SlaDueAt <= filter.SlaDueTo.Value);

            if (filter.OverdueOnly)
            {
                var now = DateTime.Now;
                q = q.Where(x =>
                    x.SlaDueAt != null &&
                    x.SlaDueAt < now &&
                    x.Status != "Fechado" &&
                    x.Status != "Cancelado");
            }

            var page = q.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(take).ToList();
            return Task.FromResult((IReadOnlyList<TicketListItemDto>)page);
        }

        public Ticket Seed(Ticket t)
        {
            typeof(Ticket).GetProperty("Id")!.SetValue(t, _nextId++);
            _tickets.Add(t);
            return t;
        }
    }
}