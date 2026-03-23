namespace HelpDesk.Application.Collaboration.UseCases.GetCommentById
{
    public sealed record GetCommentByIdQuery(int TicketId, int CommentId, int UserId);
}
