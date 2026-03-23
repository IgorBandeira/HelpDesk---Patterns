using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.Ticketing.Models;

namespace HelpDesk.Infrastructure.IdentityAccess.Models
{
    public sealed class UserEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "Requester";

        public List<TicketEntity> RequestedTickets { get; set; } = new();
        public List<TicketEntity> AssignedTickets { get; set; } = new();

    }
}
