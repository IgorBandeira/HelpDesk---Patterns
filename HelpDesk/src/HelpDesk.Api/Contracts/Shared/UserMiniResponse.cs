namespace HelpDesk.Api.Contracts.Shared
{
    public sealed class UserMiniResponse
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
