using HelpDesk.Application.Attachments.DTOs;
using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Infrastructure.Attachments.Models;
using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Ticketing.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Ticketing.Queries
{
    public sealed class TicketQueryPort : ITicketQueryPort
    {
        private readonly AppDbContext _db;

        public TicketQueryPort(AppDbContext db) => _db = db;

        public async Task<TicketDetailsDto?> GetDetailsByIdAsync(int ticketId)
        {
            var ticket = await _db.Set<TicketEntity>()
                .AsNoTracking()
                .Where(x => x.Id == ticketId)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Description,
                    x.Status,
                    Priority = x.PriorityLevel,
                    x.CreatedAt,
                    x.AssignedAt,
                    x.ClosedAt,
                    x.SlaStartAt,
                    x.SlaDueAt,

                    Requester = new UserMiniDto(
                        x.RequesterId,
                        x.Requester != null && !string.IsNullOrWhiteSpace(x.Requester.Name)
                            ? x.Requester.Name
                            : "(solicitante removido)"
                    ),

                    Assignee = new UserMiniDto(
                        x.AssigneeId,
                        x.AssigneeId == null
                            ? "(sem responsável)"
                            : x.Assignee != null && !string.IsNullOrWhiteSpace(x.Assignee.Name)
                                ? x.Assignee.Name
                                : "(sem responsável)"
                    ),

                    Category = new CategoryMiniDto(
                        x.CategoryId,
                        x.Category != null && !string.IsNullOrWhiteSpace(x.Category.Name)
                            ? x.Category.Name
                            : "(categoria removida)"
                    )
                })
                .FirstOrDefaultAsync();

            if (ticket is null)
                return null;

            var comments = await _db.Set<CommentEntity>()
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CommentDetailsDto(
                    x.Id,
                    new UserMiniDto(
                        x.AuthorId ?? 0,
                        x.Author != null && !string.IsNullOrEmpty(x.Author.Name)
                            ? x.Author.Name
                            : "(autor removido)"
                    ),
                    x.Visibility,
                    x.Message,
                    x.CreatedAt
                ))
                .ToListAsync();

            var attachments = await _db.Set<AttachmentEntity>()
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .OrderByDescending(x => x.Id)
                .Select(x => new AttachmentListItemDto(
                    x.Id,
                    x.TicketId,
                    x.FileName,
                    x.ContentType,
                    x.SizeBytes,
                    x.StorageKey,
                    x.PublicUrl,
                    x.UploadedAt,
                    new UserMiniDto(
                        x.UploadedById ?? 0,
                        x.UploadedBy != null && !string.IsNullOrWhiteSpace(x.UploadedBy.Name)
                            ? x.UploadedBy.Name
                            : "(autor removido)"
                    )
                ))
                .ToListAsync();

            var actions = await _db.Set<TicketActionEntity>()
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TicketActionDto(
                    x.Description,
                    x.CreatedAt
                ))
                .ToListAsync();

            return new TicketDetailsDto(
                ticket.Id,
                ticket.Title,
                ticket.Description,
                ticket.Status,
                ticket.Priority,
                ticket.CreatedAt,
                ticket.AssignedAt,
                ticket.ClosedAt,
                ticket.SlaStartAt,
                ticket.SlaDueAt,
                ticket.Requester,
                ticket.Assignee,
                ticket.Category,
                comments,
                attachments,
                actions
            );
        }

        public async Task<IReadOnlyList<TicketListItemDto>> ListAsync(
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
            var query = _db.Set<TicketEntity>()
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status);
            else
                query = query.Where(x => x.Status != "Cancelado");

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(x => x.PriorityLevel == priority);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(x => x.Title.Contains(title));

            if (createdFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(x => x.CreatedAt <= createdTo.Value);

            if (requesterId.HasValue)
                query = query.Where(x => x.RequesterId == requesterId.Value);

            if (assigneeId.HasValue)
                query = query.Where(x => x.AssigneeId == assigneeId.Value);

            if (categoryId.HasValue)
                query = query.Where(x => x.CategoryId == categoryId.Value);

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

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new TicketListItemDto(
                    x.Id,
                    x.Title,
                    x.Status,
                    x.PriorityLevel,
                    x.CreatedAt,
                    x.SlaDueAt,
                    new UserMiniDto(
                        x.RequesterId,
                        x.Requester != null && !string.IsNullOrWhiteSpace(x.Requester.Name)
                            ? x.Requester.Name
                            : "(solicitante removido)"
                    ),
                    new UserMiniDto(
                        x.AssigneeId,
                        x.AssigneeId == null
                            ? "(sem responsável)"
                            : x.Assignee != null && !string.IsNullOrWhiteSpace(x.Assignee.Name)
                                ? x.Assignee.Name
                                : "(sem responsável)"
                    ),
                    new CategoryMiniDto(
                        x.CategoryId,
                        x.Category != null && !string.IsNullOrWhiteSpace(x.Category.Name)
                            ? x.Category.Name
                            : "(categoria removida)"
                    )
                ))
                .ToListAsync();
        }
    }
}