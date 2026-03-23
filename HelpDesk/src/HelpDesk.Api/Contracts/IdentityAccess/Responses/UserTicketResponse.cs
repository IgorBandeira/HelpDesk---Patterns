namespace HelpDesk.Api.Contracts.IdentityAccess.Responses
{
    public sealed record UserTicketResponse(int Id, string Title, string Status, string PriorityLevel);
}
