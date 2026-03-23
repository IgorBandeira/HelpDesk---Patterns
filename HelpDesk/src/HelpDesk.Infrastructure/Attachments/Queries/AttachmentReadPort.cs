using HelpDesk.Application.Attachments.DTOs;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Infrastructure.Attachments.Models;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Attachments.Queries
{
    public sealed class AttachmentReadPort : IAttachmentReadPort
    {
        private readonly AppDbContext _db;

        public AttachmentReadPort(AppDbContext db) => _db = db;

        public async Task<AttachmentListItemDto?> GetDetailsByIdAsync(int ticketId, int attachmentId)
        {
            return await _db.Set<AttachmentEntity>()
                .AsNoTracking()
                .Include(x => x.UploadedBy)
                .Where(x => x.TicketId == ticketId && x.Id == attachmentId)
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
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<AttachmentListItemDto>> ListDetailsByTicketAsync(int ticketId)
        {
            return await _db.Set<AttachmentEntity>()
                .AsNoTracking()
                .Include(x => x.UploadedBy)
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
        }
    }
}