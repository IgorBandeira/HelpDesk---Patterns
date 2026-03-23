using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Domain.Collaboration.Aggregates;

namespace HelpDesk.UnitTests.Collaboration.Fakes
{
    public sealed class InMemoryCommentReadPort : ICommentReadPort
    {
        private readonly InMemoryCommentRepository _repo;

        public InMemoryCommentReadPort(InMemoryCommentRepository repo)
            => _repo = repo;

        public async Task<CommentDetailsDto?> GetDetailsByIdAsync(int ticketId, int commentId)
        {
            var c = await _repo.GetByIdAsync(ticketId, commentId);
            return c is null ? null : ToDto(c);
        }

        public async Task<IReadOnlyList<CommentDetailsDto>> ListDetailsByTicketAsync(int ticketId)
        {
            var all = await _repo.ListByTicketAsync(ticketId);
            return all.Select(ToDto).ToList();
        }

        private static CommentDetailsDto ToDto(TicketComment c) =>
            new(c.Id,
                new UserMiniDto(c.AuthorId, "(fake)"),
                c.Visibility,
                c.Message,
                c.CreatedAt);
    }
}