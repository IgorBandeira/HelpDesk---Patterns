using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Ticketing.Aggregates;
using HelpDesk.Domain.Ticketing.ValueObjects;
using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Domain.Collaboration.Aggregates;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Infrastructure.Ticketing.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HelpDesk.Infrastructure.Ticketing.Repositories
{
    public sealed class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _db;

        public TicketRepository(AppDbContext db) => _db = db;

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            var e = await _db.Tickets
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.Id == id);

            return e is null ? null : ToDomain(e);
        }

        public async Task AddAsync(Ticket ticket)
        {
            var e = new TicketEntity
            {
                Title = ticket.Title.Value,
                Description = ticket.Description.Value,
                Status = ticket.Status,
                PriorityLevel = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                SlaStartAt = ticket.SlaStartAt,
                AssignedAt = ticket.AssignedAt,
                ClosedAt = ticket.ClosedAt,
                SlaDueAt = ticket.SlaDueAt,
                RequesterId = ticket.RequesterId,
                AssigneeId = ticket.AssigneeId,
                CategoryId = ticket.CategoryId
            };

            _db.Tickets.Add(e);
            await _db.SaveChangesAsync();

            typeof(Ticket).GetProperty(nameof(Ticket.Id))!.SetValue(ticket, e.Id);

            if (ticket.Comments.Any())
            {
                foreach (var c in ticket.Comments)
                {
                    _db.Set<CommentEntity>().Add(new CommentEntity
                    {
                        TicketId = e.Id,
                        AuthorId = c.AuthorId,
                        Visibility = c.Visibility,
                        Message = c.Message,
                        CreatedAt = c.CreatedAt
                    });
                }

                await _db.SaveChangesAsync();
            }
        }

        public async Task SaveAsync(Ticket ticket)
        {
            var e = await _db.Tickets
                .Include(x => x.Comments)
                .FirstAsync(x => x.Id == ticket.Id);

            e.Title = ticket.Title.Value;
            e.Description = ticket.Description.Value;
            e.Status = ticket.Status;
            e.PriorityLevel = ticket.Priority;
            e.CreatedAt = ticket.CreatedAt;
            e.SlaStartAt = ticket.SlaStartAt;
            e.AssignedAt = ticket.AssignedAt;
            e.ClosedAt = ticket.ClosedAt;
            e.SlaDueAt = ticket.SlaDueAt;
            e.RequesterId = ticket.RequesterId;
            e.AssigneeId = ticket.AssigneeId;
            e.CategoryId = ticket.CategoryId;

            var existingComments = e.Comments
                .Select(x => new { x.AuthorId, x.Visibility, x.Message, x.CreatedAt })
                .ToList();

            var newComments = ticket.Comments
                .Where(c => !existingComments.Any(ec =>
                    ec.AuthorId == c.AuthorId &&
                    ec.Visibility == c.Visibility &&
                    ec.Message == c.Message &&
                    ec.CreatedAt == c.CreatedAt))
                .ToList();

            foreach (var c in newComments)
            {
                e.Comments.Add(new CommentEntity
                {
                    TicketId = ticket.Id,
                    AuthorId = c.AuthorId,
                    Visibility = c.Visibility,
                    Message = c.Message,
                    CreatedAt = c.CreatedAt
                });
            }

            await _db.SaveChangesAsync();
        }

        private static Ticket ToDomain(TicketEntity e)
        {
            var t = Ticket.CreateNew(
                TicketTitle.Create(e.Title),
                TicketDescription.Create(e.Description),
                e.PriorityLevel,
                e.RequesterId ?? 0,
                e.CategoryId ?? 0,
                e.CreatedAt);

            typeof(Ticket).GetProperty(nameof(Ticket.Id))!.SetValue(t, e.Id);

            SetPrivate(t, nameof(Ticket.Status), e.Status);
            SetPrivate(t, nameof(Ticket.CreatedAt), e.CreatedAt);
            SetPrivate(t, nameof(Ticket.SlaStartAt), e.SlaStartAt);
            SetPrivate(t, nameof(Ticket.AssignedAt), e.AssignedAt);
            SetPrivate(t, nameof(Ticket.ClosedAt), e.ClosedAt);
            SetPrivate(t, nameof(Ticket.SlaDueAt), e.SlaDueAt);
            SetPrivate(t, nameof(Ticket.AssigneeId), e.AssigneeId);

            if (e.Comments is not null && e.Comments.Count > 0)
            {
                var commentsField = typeof(Ticket)
                    .GetField("_comments", BindingFlags.Instance | BindingFlags.NonPublic);

                if (commentsField?.GetValue(t) is List<TicketComment> comments)
                {
                    foreach (var c in e.Comments.OrderBy(x => x.CreatedAt))
                    {
                        var domainComment = TicketComment.CreateNew(
                            c.TicketId,
                            c.AuthorId ?? 0,
                            c.Visibility,
                            CommentMessage.Create(c.Message),
                            c.CreatedAt);

                        typeof(TicketComment)
                            .GetProperty(nameof(TicketComment.Id))!
                            .SetValue(domainComment, c.Id);

                        comments.Add(domainComment);
                    }
                }
            }

            return t;
        }

        private static void SetPrivate(object obj, string prop, object? value)
        {
            var p = obj.GetType().GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            p!.SetValue(obj, value);
        }
    }
}