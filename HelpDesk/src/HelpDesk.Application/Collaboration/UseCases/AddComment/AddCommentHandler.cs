using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.Internal;
using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Collaboration.Aggregates;
using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.Collaboration.UseCases.AddComment
{
    public sealed class AddCommentHandler
    {
        private readonly IUserReadPort _users;
        private readonly ITicketReadPort _tickets;
        private readonly ICommentRepository _comments;
        private readonly IClock _clock;

        public AddCommentHandler(IUserReadPort users, ITicketReadPort tickets, ICommentRepository comments, IClock clock)
            => (_users, _tickets, _comments, _clock) = (users, tickets, comments, clock);

        public async Task<CommentResponseDto> HandleAsync(AddCommentCommand cmd)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.BadRequest, "Usuário inválido ou não informado.");

            var ticket = await _tickets.GetByIdAsync(cmd.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (CollaborationRules.TicketIsInactive(ticket.Status))
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível comentar em chamados inativos.");

            CommentMessage message;
            try
            {
                message = CommentMessage.Create(cmd.Dto.Message);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            var visibility = string.IsNullOrWhiteSpace(cmd.Dto.Visibility)
                ? CommentVisibility.Public
                : cmd.Dto.Visibility!;

            if (visibility != CommentVisibility.Public && visibility != CommentVisibility.Internal)
                throw new AppException(HttpStatusCodes.BadRequest, "Visibilidade inválida. Use 'Público' ou 'Interno'.");

            if (visibility == CommentVisibility.Internal)
            {
                var allowed = CollaborationRules.Participants(user.Id, user.Role, ticket.RequesterId, ticket.AssigneeId);
                if (!allowed)
                    throw new AppException(HttpStatusCodes.Forbidden,
                        "Somente requester e assignee do chamado em questão ou manager podem criar comentários internos.");
            }

            var comment = TicketComment.CreateNew(cmd.TicketId, cmd.UserId, visibility, message, _clock.Now);
            await _comments.AddAsync(comment);

            return new CommentResponseDto(comment.Id, comment.AuthorId, comment.Visibility, comment.Message, comment.CreatedAt);
        }
    }
}
