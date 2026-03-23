using HelpDesk.Api.Contracts.Shared;
using HelpDesk.Api.Contracts.Ticketing.Responses;
using HelpDesk.Application.Shared.DTOs;

namespace HelpDesk.Api.Mapping.Shared
{
    public static class SharedApiMapping
    {
        public static UserMiniResponse ToResponse(this UserMiniDto dto)
        {
            return new UserMiniResponse
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }

        public static CategoryMiniResponse ToResponse(this CategoryMiniDto dto)
        {
            return new CategoryMiniResponse
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}