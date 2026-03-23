using HelpDesk.Api.Contracts.Attachments.Requests;
using HelpDesk.Api.Contracts.Attachments.Responses;
using HelpDesk.Api.Mapping.Collaboration;
using HelpDesk.Api.Mapping.Shared;
using HelpDesk.Application.Attachments.DTOs;

namespace HelpDesk.Api.Mapping.Attachments
{
    public static class AttachmentApiMapping
    {
        public static UploadAttachmentDto ToApplicationDto(this UploadAttachmentRequest request)
        {
            return new UploadAttachmentDto(
                request.FileName ?? string.Empty,
                request.ContentType,
                request.SizeBytes
            );
        }

        public static AttachmentCreatedResponse ToCreatedResponse(this AttachmentResponseDto dto)
        {
            return new AttachmentCreatedResponse
            {
                Id = dto.Id,
                TicketId = dto.TicketId,
                FileName = dto.FileName,
                ContentType = dto.ContentType,
                SizeBytes = dto.SizeBytes,
                StorageKey = dto.StorageKey,
                PublicUrl = dto.PublicUrl,
                UploadedAt = dto.UploadedAt,
                UploadedById = dto.UploadedById
            };
        }

        public static AttachmentItemResponse ToResponse(this AttachmentListItemDto dto)
        {
            return new AttachmentItemResponse
            {
                Id = dto.Id,
                TicketId = dto.TicketId,
                FileName = dto.FileName,
                ContentType = dto.ContentType,
                SizeBytes = dto.SizeBytes,
                StorageKey = dto.StorageKey,
                PublicUrl = dto.PublicUrl,
                UploadedAt = dto.UploadedAt,
                UploadedBy = dto.UploadedBy.ToResponse()
            };
        }

        public static IReadOnlyList<AttachmentItemResponse> ToResponseList(
            this IReadOnlyList<AttachmentListItemDto> items)
        {
            return items.Select(x => x.ToResponse()).ToList();
        }
    }
}