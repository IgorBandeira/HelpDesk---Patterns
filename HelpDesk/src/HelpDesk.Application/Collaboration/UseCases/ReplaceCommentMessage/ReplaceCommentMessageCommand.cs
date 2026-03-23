using HelpDesk.Application.Collaboration.DTOs;

namespace HelpDesk.Application.Collaboration.UseCases.ReplaceCommentMessage
{
    public sealed record ReplaceCommentMessageCommand(int TicketId, int CommentId, int UserId, UpdateCommentMessageDto Dto);
}
