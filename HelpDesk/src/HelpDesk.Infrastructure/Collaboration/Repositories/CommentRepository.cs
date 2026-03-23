using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Domain.Collaboration.Aggregates;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Collaboration.Repositories
{
    public sealed class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _db;

        public CommentRepository(AppDbContext db) => _db = db;

        public async Task<TicketComment?> GetByIdAsync(int ticketId, int commentId)
        {
            var entity = await _db.Set<CommentEntity>()
                .FirstOrDefaultAsync(x => x.TicketId == ticketId && x.Id == commentId);

            return entity is null ? null : ToDomain(entity);
        }

        public async Task AddAsync(TicketComment comment)
        {
            var entity = new CommentEntity
            {
                TicketId = comment.TicketId,
                AuthorId = comment.AuthorId,
                Visibility = comment.Visibility,
                Message = comment.Message,
                CreatedAt = comment.CreatedAt
            };

            _db.Set<CommentEntity>().Add(entity);
            await _db.SaveChangesAsync();

            typeof(TicketComment)
                .GetProperty(nameof(TicketComment.Id))!
                .SetValue(comment, entity.Id);
        }

        public async Task SaveAsync(TicketComment comment)
        {
            var entity = await _db.Set<CommentEntity>()
                .FirstAsync(x => x.TicketId == comment.TicketId && x.Id == comment.Id);

            entity.Message = comment.Message;
            entity.Visibility = comment.Visibility;
            entity.CreatedAt = comment.CreatedAt;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(TicketComment comment)
        {
            var entity = await _db.Set<CommentEntity>()
                .FirstAsync(x => x.TicketId == comment.TicketId && x.Id == comment.Id);

            _db.Set<CommentEntity>().Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<TicketComment>> ListByTicketAsync(int ticketId)
        {
            var items = await _db.Set<CommentEntity>()
                .AsNoTracking()
                .Where(x => x.TicketId == ticketId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return items.Select(ToDomain).ToList();
        }

        private static TicketComment ToDomain(CommentEntity entity)
        {
            if (!entity.AuthorId.HasValue)
                throw new InvalidOperationException("Comentário sem autor não pode ser carregado como agregado de domínio.");

            var message = CommentMessage.Create(entity.Message);

            var comment = TicketComment.CreateNew(
                entity.TicketId,
                entity.AuthorId.Value,
                entity.Visibility,
                message,
                entity.CreatedAt);

            typeof(TicketComment)
                .GetProperty(nameof(TicketComment.Id))!
                .SetValue(comment, entity.Id);

            return comment;
        }
    }
}