using HelpDesk.Application.Attachments.DTOs;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.Application.Attachments.UseCases.ListAttachments
{
    public sealed class ListAttachmentsHandler
    {
        private readonly ITicketReadPort _tickets;
        private readonly IAttachmentRepository _attachments;
        private readonly IUserReadPort _users;

        public ListAttachmentsHandler(ITicketReadPort tickets, IAttachmentRepository attachments, IUserReadPort users)
            => (_tickets, _attachments, _users) = (tickets, attachments, users);

        public async Task<IReadOnlyList<AttachmentListItemDto>> HandleAsync(ListAttachmentsQuery q)
        {
            var ticket = await _tickets.GetByIdAsync(q.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            var items = await _attachments.ListByTicketAsync(q.TicketId);

            var ordered = items.OrderByDescending(a => a.Id).ToList();

            var result = new List<AttachmentListItemDto>(ordered.Count);

            foreach (var a in ordered)
            {
                var u = await _users.GetByIdAsync(a.UploadedById);
                var name = (u is not null && !string.IsNullOrWhiteSpace(u.Name))
                    ? u.Name
                    : "(autor removido)";

                result.Add(new AttachmentListItemDto(
                a.Id,
                a.TicketId,
                a.FileName,
                a.ContentType,
                a.SizeBytes,
                a.StorageKey,
                a.PublicUrl,
                a.UploadedAt,
                new UserMiniDto(a.UploadedById, name)
                ));
            }

            return result;
        }
    }
}
