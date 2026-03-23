namespace HelpDesk.Api.Contracts.Ticketing.Requests
{
    public sealed class ChangeStatusRequest
    {
        public string NewStatus { get; init; } = "";
    }
}