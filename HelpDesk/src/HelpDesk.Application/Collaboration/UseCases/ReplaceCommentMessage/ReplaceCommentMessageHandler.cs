using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.Internal;
using HelpDesk.Application.Collaboration.Ports;
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
        private readonly IClock _clock;

        public ReplaceCommentMessageHandler(
            ITicketReadPort tickets,
            ICommentRepository comments,
            ICommentReadPort commentReads,
            IClock clock)
            => (_tickets, _comments, _commentReads, _clock) = (tickets, comments, commentReads, clock);

        public async Task<CommentDetailsDto> HandleAsync(ReplaceCommentMessageCommand cmd)
        {
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

            var c = await _comments.GetByIdAsync(cmd.TicketId, cmd.CommentId);
            if (c is null)
                throw new AppException(HttpStatusCodes.NotFound, "Comentário não encontrado.");

            if (c.AuthorId != cmd.UserId)
                throw new AppException(HttpStatusCodes.Forbidden, "Não é possível editar comentários de outras pessoas!");

            try
            {
                c.ReplaceMessage(newMessage, _clock.Now);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            await _comments.SaveAsync(c);

            var details = await _commentReads.GetDetailsByIdAsync(cmd.TicketId, cmd.CommentId);
            if (details is null)
                throw new AppException(HttpStatusCodes.NotFound, "Comentário não encontrado.");

            return details;
        }
    }
}