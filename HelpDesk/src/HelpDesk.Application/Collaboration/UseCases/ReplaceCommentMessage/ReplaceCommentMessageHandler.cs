using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.Internal;
using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.Collaboration.UseCases.ReplaceCommentMessage
{
    public sealed class ReplaceCommentMessageHandler
    {
        private readonly ITicketReadPort _tickets;
        private readonly ICommentRepository _comments;
        private readonly ICommentReadPort _commentReads;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ReplaceCommentMessageHandler(
            ITicketReadPort tickets,
            ICommentRepository comments,
            ICommentReadPort commentReads,
            IUserReadPort users,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _tickets = tickets;
            _comments = comments;
            _commentReads = commentReads;
            _users = users;
            _clock = clock;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<CommentDetailsDto> HandleAsync(ReplaceCommentMessageCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.BadRequest, "Usuário inválido ou não informado.");

            var ticket = await _tickets.GetByIdAsync(cmd.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (CollaborationRules.TicketIsInactive(ticket.Status))
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível editar comentários em tickets inativos.");

            CommentMessage newMessage;
            try
            {
                newMessage = CommentMessage.Create(cmd.Dto.Message);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            var comment = await _comments.GetByIdAsync(cmd.TicketId, cmd.CommentId);
            if (comment is null)
                throw new AppException(HttpStatusCodes.NotFound, "Comentário não encontrado.");

            if (comment.AuthorId != cmd.UserId)
                throw new AppException(HttpStatusCodes.Forbidden, "Não é possível editar comentários de outras pessoas!");

            var now = _clock.Now;
            var oldMessage = comment.Message;

            try
            {
                comment.ReplaceMessage(newMessage, now);
                comment.RaiseMessageReplacedEvent(user.Name, oldMessage, now);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            await _comments.SaveAsync(comment);

            await _domainEventDispatcher.DispatchAsync(comment.DomainEvents, ct);
            comment.ClearDomainEvents();

            var details = await _commentReads.GetDetailsByIdAsync(cmd.TicketId, cmd.CommentId);
            if (details is null)
                throw new AppException(HttpStatusCodes.NotFound, "Comentário não encontrado.");

            return details;
        }
    }
}