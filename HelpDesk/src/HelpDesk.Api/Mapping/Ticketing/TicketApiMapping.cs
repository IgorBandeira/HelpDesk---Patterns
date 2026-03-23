using HelpDesk.Api.Contracts.Ticketing.Requests;
using HelpDesk.Api.Contracts.Ticketing.Responses;
using HelpDesk.Api.Mapping.Attachments;
using HelpDesk.Api.Mapping.Collaboration;
using HelpDesk.Api.Mapping.Shared;
using HelpDesk.Application.Ticketing.DTOs;

namespace HelpDesk.Api.Mapping.Ticketing
{
    public static class TicketApiMapping
    {
        public static CreateTicketDto ToDto(this CreateTicketRequest request) => new()
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            CategoryId = request.CategoryId
        };

        public static UpdateTicketDto ToDto(this UpdateTicketRequest request) => new()
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            CategoryId = request.CategoryId
        };

        public static AssignRequestDto ToDto(this AssignTicketRequest request) => new()
        {
            AgentId = request.AgentId
        };

        public static ChangeRequesterDto ToDto(this ChangeRequesterRequest request) => new()
        {
            RequesterId = request.RequesterId
        };

        public static ChangeStatusDto ToDto(this ChangeStatusRequest request) => new()
        {
            NewStatus = request.NewStatus
        };

        public static ReopenTicketDto ToDto(this ReopenTicketRequest request) => new()
        {
            Reason = request.Reason
        };

        public static CancelTicketDto ToDto(this CancelTicketRequest request) => new()
        {
            Reason = request.Reason
        };

        public static TicketResponse ToResponse(this TicketResponseDto dto) => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Priority = dto.Priority,
            CreatedAt = dto.CreatedAt,
            SlaStartAt = dto.SlaStartAt,
            SlaDueAt = dto.SlaDueAt,
            RequesterId = dto.RequesterId,
            CategoryId = dto.CategoryId
        };

        public static TicketListItemResponse ToResponse(this TicketListItemDto dto)
        {
            return new TicketListItemResponse
            {
                Id = dto.Id,
                Title = dto.Title,
                Status = dto.Status,
                Priority = dto.Priority,
                CreatedAt = dto.CreatedAt,
                SlaDueAt = dto.SlaDueAt,
                Requester = dto.Requester.ToResponse(),
                Assignee = dto.Assignee?.ToResponse(),
                Category = dto.Category.ToResponse()
            };
        }

        public static IReadOnlyList<TicketListItemResponse> ToResponseList(
            this IReadOnlyList<TicketListItemDto> items)
        {
            return items.Select(x => x.ToResponse()).ToList();
        }

        public static TicketDetailsResponse ToResponse(this TicketDetailsDto dto)
        {
            return new TicketDetailsResponse
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                CreatedAt = dto.CreatedAt,
                AssignedAt = dto.AssignedAt,
                ClosedAt = dto.ClosedAt,
                SlaStartAt = dto.SlaStartAt,
                SlaDueAt = dto.SlaDueAt,
                Requester = dto.Requester.ToResponse(),
                Assignee = dto.Assignee?.ToResponse(),
                Category = dto.Category.ToResponse(),
                Comments = dto.Comments.Select(x => x.ToDetailsResponse()).ToList(),
                Attachments = dto.Attachments.Select(x => x.ToResponse()).ToList(),
                Actions = dto.Actions.Select(x => x.ToResponse()).ToList()
            };
        }

        public static TicketActionResponse ToResponse(this TicketActionDto dto)
        {
            return new TicketActionResponse
            {
                Description = dto.Description,
                CreatedAt = dto.CreatedAt
            };
        }

        public static AssignTicketResponse ToResponse(this AssignResponseDto dto) => new()
        {
            TicketId = dto.TicketId,
            Status = dto.Status,
            AssignedAt = dto.AssignedAt,
            AgentId = dto.AgentId,
            AgentName = dto.AgentName
        };

        public static RequesterResponse ToResponse(this RequesterResponseDto dto) => new()
        {
            TicketId = dto.TicketId,
            Status = dto.Status,
            RequesterId = dto.RequesterId,
            RequesterName = dto.RequesterName
        };

        public static ChangeStatusResponse ToResponse(this ChangeStatusResponseDto dto) => new()
        {
            TicketId = dto.TicketId,
            PreviousStatus = dto.PreviousStatus,
            NewStatus = dto.NewStatus,
            ClosedAt = dto.ClosedAt
        };

        public static ReopenTicketResponse ToResponse(this ReopenResponseDto dto) => new()
        {
            TicketId = dto.TicketId,
            PreviousStatus = dto.PreviousStatus,
            NewStatus = dto.NewStatus,
            ReopenedAt = dto.ReopenedAt,
            ActorUserId = dto.ActorUserId,
            Reason = dto.Reason
        };

        public static CancelTicketResponse ToResponse(this CancelResponseDto dto) => new()
        {
            TicketId = dto.TicketId,
            PreviousStatus = dto.PreviousStatus,
            NewStatus = dto.NewStatus,
            ClosedAt = dto.ClosedAt,
            ActorUserId = dto.ActorUserId,
            Reason = dto.Reason
        };
    }
}