using HelpDesk.Application.Collaboration.Internal;
using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.Application.Collaboration.UseCases.DeleteComment
{
    public sealed class DeleteCommentHandler
    {
        private readonly ITicketReadPort _tickets;
        private readonly ICommentRepository _comments;

        public DeleteCommentHandler(ITicketReadPort tickets, ICommentRepository comments)
            => (_tickets, _comments) = (tickets, comments);

        public async Task HandleAsync(DeleteCommentCommand cmd)
        {
            var ticket = await _tickets.GetByIdAsync(cmd.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (CollaborationRules.TicketIsInactive(ticket.Status))
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível excluir comentários em chamados inativos.");

            var c = await _comments.GetByIdAsync(cmd.TicketId, cmd.CommentId);
            if (c is null)
                throw new AppException(HttpStatusCodes.NotFound, "Comentário não encontrado.");

            if (c.AuthorId != cmd.UserId)
                throw new AppException(HttpStatusCodes.Forbidden, "Não é possível excluir comentários de outras pessoas!");

            await _comments.DeleteAsync(c);
        }
    }
}
