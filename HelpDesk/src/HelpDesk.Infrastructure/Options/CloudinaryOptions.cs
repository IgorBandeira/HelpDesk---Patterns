namespace HelpDesk.Infrastructure.Options
{
    public sealed class CloudinaryOptions
    {
        public string CloudName { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string ApiSecret { get; set; } = "";
        public string Prefix { get; set; } = "";
        public string Folder { get; set; } = "helpdesk";
    }
}