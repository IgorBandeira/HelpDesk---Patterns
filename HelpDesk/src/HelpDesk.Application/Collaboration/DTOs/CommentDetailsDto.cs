using HelpDesk.Application.Shared.DTOs;

namespace HelpDesk.Application.Collaboration.DTOs
{
    public sealed record CommentDetailsDto(
        int Id,
        UserMiniDto Author,
        string Visibility,
        string Message,
        DateTime CreatedAt
    );
}
