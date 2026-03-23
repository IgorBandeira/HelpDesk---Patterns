using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Application.Ticketing.UseCases.AssignTicket
{
    public sealed record AssignTicketCommand(int Id, int UserId, DTOs.AssignRequestDto Dto);

}
