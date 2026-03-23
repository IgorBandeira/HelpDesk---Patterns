using HelpDesk.Api.Contracts.Collaboration.Requests;
using HelpDesk.Api.Contracts.Collaboration.Responses;
using HelpDesk.Api.Mapping.Shared;
using HelpDesk.Application.Collaboration.DTOs;

namespace HelpDesk.Api.Mapping.Collaboration
{
    public static class CommentApiMapping
    {
        public static AddCommentDto ToApplicationDto(this AddCommentRequest request)
        {
            return new AddCommentDto(
                request.Message,
                request.Visibility);
        }

        public static UpdateCommentMessageDto ToApplicationDto(this ReplaceCommentMessageRequest request)
        {
            return new UpdateCommentMessageDto(request.Message);
        }

        public static CommentResponse ToResponse(this CommentResponseDto dto)
        {
            return new CommentResponse
            {
                Id = dto.Id,
                AuthorId = dto.AuthorId,
                Visibility = dto.Visibility,
                Message = dto.Message,
                CreatedAt = dto.CreatedAt
            };
        }

        public static CommentDetailsResponse ToDetailsResponse(this CommentDetailsDto dto)
        {
            return new CommentDetailsResponse
            {
                Id = dto.Id,
                Author = dto.Author.ToResponse(),
                Visibility = dto.Visibility,
                Message = dto.Message,
                CreatedAt = dto.CreatedAt
            };
        }

        public static IReadOnlyList<CommentDetailsResponse> ToResponseList(this IReadOnlyList<CommentDetailsDto> items)
        {
            return items.Select(x => x.ToDetailsResponse()).ToList();
        }
    }
}