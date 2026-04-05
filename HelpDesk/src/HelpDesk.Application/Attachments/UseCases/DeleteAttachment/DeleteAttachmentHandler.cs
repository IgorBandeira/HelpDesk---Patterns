using HelpDesk.Application.Attachments.Internal;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.Attachments.UseCases.DeleteAttachment
{
    public sealed class DeleteAttachmentHandler
    {
        private readonly ITicketReadPort _tickets;
        private readonly IAttachmentRepository _attachments;
        private readonly IUserReadPort _users;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public DeleteAttachmentHandler(
            ITicketReadPort tickets,
            IAttachmentRepository attachments,
            IUserReadPort users,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _tickets = tickets;
            _attachments = attachments;
            _users = users;
            _clock = clock;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task HandleAsync(DeleteAttachmentCommand cmd, CancellationToken ct = default)
        {
            var ticket = await _tickets.GetByIdAsync(cmd.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (AttachmentTicketRules.IsInactive(ticket.Status))
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível excluir anexos em tickets inativos.");

            var att = await _attachments.GetByIdAsync(cmd.TicketId, cmd.AttachmentId);
            if (att is null)
                throw new AppException(HttpStatusCodes.NotFound, "Anexo não encontrado.");

            try
            {
                att.EnsureCanBeDeletedBy(cmd.UserId);
            }
            catch (DomainException ex)
            {
                if (ex.Message == "Não é possível excluir anexos de outras pessoas!")
                    throw new AppException(HttpStatusCodes.Forbidden, ex.Message);

                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            var user = await _users.GetByIdAsync(cmd.UserId);
            var actorUserName = (user is not null && !string.IsNullOrWhiteSpace(user.Name))
                ? user.Name
                : "(autor removido)";

            att.RaiseDeletedEvent(actorUserName, _clock.Now);

            await _domainEventDispatcher.DispatchAsync(att.DomainEvents, ct);
            att.ClearDomainEvents();

            await _attachments.DeleteAsync(att);
        }
    }
}