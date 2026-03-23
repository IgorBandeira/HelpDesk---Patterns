using HelpDesk.Api.Contracts.IdentityAccess.Responses;
using HelpDesk.Application.IdentityAccess.DTOs;

namespace HelpDesk.Api.Mapping.IdentityAccess
{
    public static class UserApiMapping
    {
        public static UserResponse ToResponse(UserDto dto)
            => new(dto.Id, dto.Name, dto.Email, dto.Role);

        public static UserWithTicketsResponse ToWithTicketsResponse(UserWithTicketsDto dto)
            => new(
                dto.Id,
                dto.Name,
                dto.Email,
                dto.Role,
                dto.RequestedTickets.Select(t => new UserTicketResponse(t.Id, t.Title, t.Status, t.PriorityLevel)),
                dto.AssignedTickets.Select(t => new UserTicketResponse(t.Id, t.Title, t.Status, t.PriorityLevel))
            );
    }
}
