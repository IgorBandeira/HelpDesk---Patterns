namespace HelpDesk.Api.Contracts.Collaboration.Requests
{
    public sealed class AddCommentRequest
    {
        public string? Message { get; set; }
        public string? Visibility { get; set; }
    }
}
