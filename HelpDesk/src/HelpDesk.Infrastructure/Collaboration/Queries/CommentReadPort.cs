using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Collaboration.Queries
{
    public sealed class CommentReadPort : ICommentReadPort
    {
        private readonly AppDbContext _db;

        public CommentReadPort(AppDbContext db) => _db = db;

        public async Task<CommentDetailsDto?> GetDetailsByIdAsync(int ticketId, int commentId)
        {
            return await _db.Set<CommentEntity>()
                .AsNoTracking()
                .Include(x => x.Author)
                .Where(x => x.TicketId == ticketId && x.Id == commentId)
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
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<CommentDetailsDto>> ListDetailsByTicketAsync(int ticketId)
        {
            return await _db.Set<CommentEntity>()
                .AsNoTracking()
                .Include(x => x.Author)
                .Where(x => x.TicketId == ticketId)
                .OrderByDescending(x => x.Id)
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
        }
    }
}