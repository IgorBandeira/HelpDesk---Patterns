namespace HelpDesk.Application.Ticketing.UseCases.ChangeStatus
{
    public sealed record ChangeStatusCommand(int Id, int UserId, DTOs.ChangeStatusDto Dto);
}
