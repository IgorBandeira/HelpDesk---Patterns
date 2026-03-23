using HelpDesk.Application.Collaboration.DTOs;

namespace HelpDesk.Application.Collaboration.Ports
{
    public interface ICommentReadPort
    {
        Task<CommentDetailsDto?> GetDetailsByIdAsync(int ticketId, int commentId);
        Task<IReadOnlyList<CommentDetailsDto>> ListDetailsByTicketAsync(int ticketId);
    }
}
