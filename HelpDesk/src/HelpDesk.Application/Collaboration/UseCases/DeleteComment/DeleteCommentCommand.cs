namespace HelpDesk.Application.Collaboration.UseCases.DeleteComment
{
    public sealed record DeleteCommentCommand(int TicketId, int CommentId, int UserId);
}
