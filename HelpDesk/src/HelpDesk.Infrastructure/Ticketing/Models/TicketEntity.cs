using HelpDesk.Infrastructure.Attachments.Models;
using HelpDesk.Infrastructure.Collaboration.Models;
using HelpDesk.Infrastructure.IdentityAccess.Models;
using HelpDesk.Infrastructure.Operations.Models;
using HelpDesk.Infrastructure.ServiceCatalog.Models;

namespace HelpDesk.Infrastructure.Ticketing.Models
{
    public sealed class TicketEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PriorityLevel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime SlaStartAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? SlaDueAt { get; set; }

        public int? RequesterId { get; set; }
        public int? AssigneeId { get; set; }
        public int? CategoryId { get; set; }

        public UserEntity? Requester { get; set; }
        public UserEntity? Assignee { get; set; }
        public CategoryEntity? Category { get; set; }

        public List<CommentEntity> Comments { get; set; } = new();
        public List<AttachmentEntity> Attachments { get; set; } = new();
        public List<TicketActionEntity> Actions { get; set; } = new();
    }
}