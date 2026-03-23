using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Domain.Attachments.Aggregates;
using HelpDesk.Domain.Attachments.ValueObjects;
using HelpDesk.Infrastructure.Attachments.Models;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Attachments.Repositories
{
    public sealed class AttachmentRepository : IAttachmentRepository
    {
        private readonly AppDbContext _db;

        public AttachmentRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Attachment attachment)
        {
            var entity = new AttachmentEntity
            {
                TicketId = attachment.TicketId,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                SizeBytes = attachment.SizeBytes,
                StorageKey = attachment.StorageKey,
                PublicUrl = attachment.PublicUrl,
                UploadedById = attachment.UploadedById,
                UploadedAt = attachment.UploadedAt
            };

            _db.Set<AttachmentEntity>().Add(entity);
            await _db.SaveChangesAsync();

            typeof(Attachment)
                .GetProperty(nameof(Attachment.Id))!
                .SetValue(attachment, entity.Id);
        }

        public async Task DeleteAsync(Attachment attachment)
        {
            var entity = await _db.Set<AttachmentEntity>()
                .FirstAsync(x => x.Id == attachment.Id && x.TicketId == attachment.TicketId);

            _db.Set<AttachmentEntity>().Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<Attachment?> GetByIdAsync(int ticketId, int attachmentId)
        {
            var entity = await _db.Set<AttachmentEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TicketId == ticketId && x.Id == attachmentId);

            return entity is null ? null : ToDomain(entity);
        }

        public async Task<IReadOnlyList<Attachment>> ListByTicketAsync(int ticketId)
        {
            var entities = await _db.Set<AttachmentEntity>()
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .ToListAsync();

            return entities.Select(ToDomain).ToList();
        }

        private static Attachment ToDomain(AttachmentEntity entity)
        {
            if (!entity.UploadedById.HasValue)
                throw new InvalidOperationException("Anexo sem autor não pode ser carregado como agregado de domínio.");

            var file = AttachmentFile.Create(entity.FileName, entity.ContentType, entity.SizeBytes);
            var storageKey = StorageKey.Create(entity.StorageKey);

            var attachment = Attachment.CreateNew(
                entity.TicketId,
                file,
                storageKey,
                entity.PublicUrl,
                entity.UploadedById.Value,
                entity.UploadedAt);

            typeof(Attachment)
                .GetProperty(nameof(Attachment.Id))!
                .SetValue(attachment, entity.Id);

            return attachment;
        }
    }
}