using HelpDesk.Application.Collaboration.DTOs;
using HelpDesk.Application.Collaboration.Internal;
using HelpDesk.Application.Collaboration.Ports;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Collaboration.Enums;

namespace HelpDesk.Application.Collaboration.UseCases.GetCommentById
{
    public sealed class GetCommentByIdHandler
    {
        private readonly IUserReadPort _users;
        private readonly ITicketReadPort _tickets;
        private readonly ICommentReadPort _comments;

        public GetCommentByIdHandler(IUserReadPort users, ITicketReadPort tickets, ICommentReadPort comments)
            => (_users, _tickets, _comments) = (users, tickets, comments);

        public async Task<CommentDetailsDto> HandleAsync(GetCommentByIdQuery q)
        {
            var user = await _users.GetByIdAsync(q.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var ticket = await _tickets.GetByIdAsync(q.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            var canSeeInternal = CollaborationRules.Participants(user.Id, user.Role, ticket.RequesterId, ticket.AssigneeId);

            var c = await _comments.GetDetailsByIdAsync(q.TicketId, q.CommentId);
            if (c is null)
                throw new AppException(HttpStatusCodes.NotFound, "Comentário não encontrado.");

            if (!canSeeInternal && c.Visibility == CommentVisibility.Internal)
                throw new AppException(HttpStatusCodes.NotFound, "Comentário não encontrado.");

            return c;
        }
    }
}