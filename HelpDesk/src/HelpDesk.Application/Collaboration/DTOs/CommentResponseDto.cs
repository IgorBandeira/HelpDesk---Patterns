namespace HelpDesk.Application.Collaboration.DTOs
{
    public sealed record CommentResponseDto(
        int Id,
        int AuthorId,
        string Visibility,
        string Message,
        DateTime CreatedAt
    );
}
