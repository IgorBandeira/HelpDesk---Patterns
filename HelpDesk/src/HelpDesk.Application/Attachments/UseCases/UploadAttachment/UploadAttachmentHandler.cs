using HelpDesk.Application.Attachments.DTOs;
using HelpDesk.Application.Attachments.Internal;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.Attachments.Aggregates;
using HelpDesk.Domain.Attachments.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.Attachments.UseCases.UploadAttachment
{
    public sealed class UploadAttachmentHandler
    {
        private readonly ITicketReadPort _tickets;
        private readonly IUserReadPort _users;
        private readonly IAttachmentRepository _attachments;
        private readonly IFileStoragePort _storage;
        private readonly IClock _clock;

        public UploadAttachmentHandler(
            ITicketReadPort tickets,
            IUserReadPort users,
            IAttachmentRepository attachments,
            IFileStoragePort storage,
            IClock clock)
            => (_tickets, _users, _attachments, _storage, _clock) =
               (tickets, users, attachments, storage, clock);

        public async Task<AttachmentResponseDto> HandleAsync(UploadAttachmentCommand cmd)
        {
            AttachmentFile fileVo;
            try
            {
                fileVo = AttachmentFile.Create(
                    cmd.File.FileName,
                    cmd.File.ContentType,
                    cmd.File.SizeBytes);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            var ticket = await _tickets.GetByIdAsync(cmd.TicketId);
            if (ticket is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (AttachmentTicketRules.IsInactive(ticket.Status))
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível anexar arquivos em chamados inativos.");

            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.BadRequest, "Usuário inválido ou não informado.");

            var key = $"{cmd.TicketId}/{fileVo.FileName}";

            var (storedKey, url) = await _storage.SaveAsync(cmd.File, key);

            StorageKey storageKey;
            try
            {
                storageKey = StorageKey.Create(storedKey);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            var now = _clock.Now;

            var att = Attachment.CreateNew(
                cmd.TicketId,
                fileVo,
                storageKey,
                url,
                cmd.UserId,
                now);

            await _attachments.AddAsync(att);

            return new AttachmentResponseDto(
                att.Id,
                att.TicketId,
                att.FileName,
                att.ContentType,
                att.SizeBytes,
                att.StorageKey,
                att.PublicUrl,
                att.UploadedAt,
                att.UploadedById);
        }
    }
}