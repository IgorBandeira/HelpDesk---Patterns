using HelpDesk.Api.Contracts.Shared;

namespace HelpDesk.Api.Contracts.Collaboration.Responses
{
    public sealed class CommentDetailsResponse
    {
        public int Id { get; set; }
        public UserMiniResponse Author { get; set; } = new();
        public string Visibility { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}