using HelpDesk.Application.Attachments.DTOs;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.DTOs;
using HelpDesk.Application.Shared.Errors;

namespace HelpDesk.Application.Attachments.UseCases.GetAttachmentById
{
    public sealed class GetAttachmentByIdHandler
    {
        private readonly IAttachmentRepository _attachments;
        private readonly IUserReadPort _users;

        public GetAttachmentByIdHandler(IAttachmentRepository attachments, IUserReadPort users)
            => (_attachments, _users) = (attachments, users);

        public async Task<AttachmentListItemDto> HandleAsync(GetAttachmentByIdQuery q)
        {
            var a = await _attachments.GetByIdAsync(q.TicketId, q.AttachmentId);
            if (a is null)
                throw new AppException(HttpStatusCodes.NotFound, "Anexo não encontrado.");

            var u = await _users.GetByIdAsync(a.UploadedById);
            var name = (u is not null && !string.IsNullOrWhiteSpace(u.Name))
                ? u.Name
                : "(autor removido)";

            return new AttachmentListItemDto(
                a.Id,
                a.TicketId,
                a.FileName,
                a.ContentType,
                a.SizeBytes,
                a.StorageKey,
                a.PublicUrl,
                a.UploadedAt,
                new UserMiniDto(a.UploadedById, name)
            );
        }
    }
}