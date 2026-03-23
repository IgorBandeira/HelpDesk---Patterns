using HelpDesk.Application.Collaboration.DTOs;

namespace HelpDesk.Application.Collaboration.UseCases.AddComment
{
    public sealed record AddCommentCommand(int TicketId, int UserId, AddCommentDto Dto);
}
