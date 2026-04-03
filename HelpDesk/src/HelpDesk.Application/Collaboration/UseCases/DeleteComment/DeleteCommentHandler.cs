using HelpDesk.Application.Collaboration.Internal;
using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.Application.Collaboration.UseCases.DeleteComment
{
    public sealed class DeleteCommentHandler
    {
        private readonly ITicketReadPort _tickets;
        private readonly ICommentRepository _comments;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public DeleteCommentHandler(
            ITicketReadPort tickets,
            ICommentRepository comments,
            IUserReadPort users,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _tickets = tickets;
            _comments = comments;
            _users = users;
            _clock = clock;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task HandleAsync(DeleteCommentCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.BadRequest, "Usuário inválido ou não informado.");

            var ticket = await _tickets.GetByIdAsync(cmd.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (CollaborationRules.TicketIsInactive(ticket.Status))
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível excluir comentários em chamados inativos.");

            var comment = await _comments.GetByIdAsync(cmd.TicketId, cmd.CommentId);
            if (comment is null)
                throw new AppException(HttpStatusCodes.NotFound, "Comentário não encontrado.");

            if (comment.AuthorId != cmd.UserId)
                throw new AppException(HttpStatusCodes.Forbidden, "Não é possível excluir comentários de outras pessoas!");

            var now = _clock.Now;

            comment.RaiseDeletedEvent(user.Name, now);

            await _comments.DeleteAsync(comment);

            await _domainEventDispatcher.DispatchAsync(comment.DomainEvents, ct);
            comment.ClearDomainEvents();
        }
    }
}