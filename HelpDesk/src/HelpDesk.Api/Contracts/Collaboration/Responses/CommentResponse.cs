namespace HelpDesk.Api.Contracts.Collaboration.Responses
{
    public sealed class CommentResponse
    {
        public int Id { get; init; }
        public int AuthorId { get; init; }
        public string Visibility { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}