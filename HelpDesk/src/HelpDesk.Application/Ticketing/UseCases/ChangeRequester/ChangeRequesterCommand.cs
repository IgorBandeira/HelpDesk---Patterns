namespace HelpDesk.Application.Ticketing.UseCases.ChangeRequester
{
    public sealed record ChangeRequesterCommand(int Id, int UserId, DTOs.ChangeRequesterDto Dto);

}
