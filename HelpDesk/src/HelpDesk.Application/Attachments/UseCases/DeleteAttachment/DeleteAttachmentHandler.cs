using HelpDesk.Application.Attachments.Internal;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.Application.Attachments.UseCases.DeleteAttachment
{
    public sealed class DeleteAttachmentHandler
    {
        private readonly ITicketReadPort _tickets;
        private readonly IAttachmentRepository _attachments;
        private readonly IFileStoragePort _storage;

        public DeleteAttachmentHandler(
            ITicketReadPort tickets,
            IAttachmentRepository attachments,
            IFileStoragePort storage)
            => (_tickets, _attachments, _storage) = (tickets, attachments, storage);

        public async Task HandleAsync(DeleteAttachmentCommand cmd)
        {
            var ticket = await _tickets.GetByIdAsync(cmd.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (AttachmentTicketRules.IsInactive(ticket.Status))
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível excluir anexos em tickets inativos.");

            var att = await _attachments.GetByIdAsync(cmd.TicketId, cmd.AttachmentId);
            if (att is null)
                throw new AppException(HttpStatusCodes.NotFound, "Anexo não encontrado.");

            if (att.UploadedById != cmd.UserId)
                throw new AppException(HttpStatusCodes.Forbidden, "Não é possível excluir anexos de outras pessoas!");

            if (!string.IsNullOrWhiteSpace(att.StorageKey))
                await _storage.DeleteAsync(att.StorageKey);

            await _attachments.DeleteAsync(att);
        }
    }
}