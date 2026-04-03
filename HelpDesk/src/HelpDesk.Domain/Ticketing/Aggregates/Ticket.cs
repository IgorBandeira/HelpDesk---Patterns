using HelpDesk.Domain.Collaboration.Aggregates;
using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.SharedKernel.Primitives;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Ticketing.Events;
using HelpDesk.Domain.Ticketing.ValueObjects;

namespace HelpDesk.Domain.Ticketing.Aggregates
{
    public sealed class Ticket : AggregateRoot<int>
    {
        public TicketTitle Title { get; private set; } = default!;
        public TicketDescription Description { get; private set; } = default!;
        public string Status { get; private set; } = TicketStatus.Novo;
        public string Priority { get; private set; } = TicketPriority.Media;

        public DateTime CreatedAt { get; private set; }
        public DateTime? AssignedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public DateTime SlaStartAt { get; private set; }
        public DateTime? SlaDueAt { get; private set; }

        public int RequesterId { get; private set; }
        public int? AssigneeId { get; private set; }
        public int CategoryId { get; private set; }

        private readonly List<TicketComment> _comments = new();
        public IReadOnlyCollection<TicketComment> Comments => _comments.AsReadOnly();

        private Ticket() { }

        private Ticket(
            int id,
            TicketTitle title,
            TicketDescription description,
            string status,
            string priority,
            DateTime createdAt,
            DateTime? assignedAt,
            DateTime? closedAt,
            DateTime slaStartAt,
            DateTime? slaDueAt,
            int requesterId,
            int? assigneeId,
            int categoryId) : base(id)
        {
            Title = title;
            Description = description;
            Status = status;
            Priority = priority;
            CreatedAt = createdAt;
            AssignedAt = assignedAt;
            ClosedAt = closedAt;
            SlaStartAt = slaStartAt;
            SlaDueAt = slaDueAt;
            RequesterId = requesterId;
            AssigneeId = assigneeId;
            CategoryId = categoryId;
        }

        public static Ticket CreateNew(
            TicketTitle title,
            TicketDescription description,
            string priority,
            int requesterId,
            int categoryId,
            DateTime now)
        {
            TicketPriority.EnsureValid(priority);

            return new Ticket(
                id: 0,
                title: title,
                description: description,
                status: TicketStatus.Novo,
                priority: priority,
                createdAt: now,
                assignedAt: null,
                closedAt: null,
                slaStartAt: now,
                slaDueAt: CalculateSlaDueAt(priority, now),
                requesterId: requesterId,
                assigneeId: null,
                categoryId: categoryId);
        }

        public bool Update(
            TicketTitle? newTitle,
            TicketDescription? newDescription,
            string? newPriority,
            int? newCategoryId,
            DateTime now)
        {
            EnsureIsActiveForEditing();

            var changed = false;

            if (newTitle is not null && !Title.Equals(newTitle))
            {
                Title = newTitle;
                changed = true;
            }

            if (newDescription is not null && !Description.Equals(newDescription))
            {
                Description = newDescription;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(newPriority) &&
                !string.Equals(Priority, newPriority, StringComparison.OrdinalIgnoreCase))
            {
                TicketPriority.EnsureValid(newPriority);

                Priority = newPriority;
                SlaStartAt = now;
                SlaDueAt = CalculateSlaDueAt(newPriority, now);
                changed = true;
            }

            if (newCategoryId.HasValue && CategoryId != newCategoryId.Value)
            {
                CategoryId = newCategoryId.Value;
                changed = true;
            }

            return changed;
        }

        public void AssignToAgent(int agentId, DateTime now)
        {
            if (agentId <= 0)
                throw new DomainException("Agent inválido.");

            EnsureIsActiveForAssignment();

            AssigneeId = agentId;
            AssignedAt = now;

            if (Status == TicketStatus.Novo)
                Status = TicketStatus.EmAnalise;
        }

        public void ChangeRequester(int requesterId)
        {
            if (requesterId <= 0)
                throw new DomainException("Requester inválido.");

            EnsureIsActiveForRequest();

            RequesterId = requesterId;
        }

        public void ChangeStatus(string nextStatus, int actorUserId, DateTime now)
        {
            var cur = Status;

            bool transitionAllowed = (cur, nextStatus) switch
            {
                (TicketStatus.EmAnalise, TicketStatus.EmAndamento) => true,
                (TicketStatus.EmAndamento, TicketStatus.Resolvido) => true,
                (TicketStatus.Resolvido, TicketStatus.Fechado) => true,
                _ => false
            };

            if (!transitionAllowed)
                throw new DomainException($"Transição inválida: {cur} -> {nextStatus}.\nEssas são as opções válidas: Em Análise -> Em Andamento -> Resolvido -> Fechado");

            bool requiresAgent =
                (cur, nextStatus) is (TicketStatus.EmAnalise, TicketStatus.EmAndamento) ||
                (cur, nextStatus) is (TicketStatus.EmAndamento, TicketStatus.Resolvido);

            bool requiresRequester =
                (cur, nextStatus) is (TicketStatus.Resolvido, TicketStatus.Fechado);

            if (requiresAgent)
            {
                if (AssigneeId is null)
                    throw new DomainException("Não há agent atribuído a este chamado para executar essa transição.");

                if (AssigneeId.Value != actorUserId)
                    throw new DomainException($"Usuário não permitido para atualizar {cur} -> {nextStatus}.");
            }

            if (requiresRequester)
            {
                if (RequesterId == 0)
                    throw new DomainException("Não há requester atribuído a este chamado para executar essa transição.");

                if (RequesterId != actorUserId)
                    throw new DomainException($"Usuário não permitido para atualizar {cur} -> {nextStatus}.");
            }

            Status = nextStatus;

            if (nextStatus == TicketStatus.Fechado)
                ClosedAt = now;
        }


        public void Cancel(int actorUserId, string reason, DateTime now)
        {
            if (actorUserId <= 0)
                throw new DomainException("Usuário inválido.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("O motivo do cancelamento é obrigatório.");

            if (Status is not (TicketStatus.Novo or TicketStatus.EmAnalise))
                throw new DomainException("Só é possível cancelar chamados em Novo ou Em Análise.");

            Status = TicketStatus.Cancelado;
            ClosedAt = now;

            AddComment(
                actorUserId,
                CommentVisibility.Internal,
                $"Chamado cancelado: {reason.Trim()}",
                now);
        }

        public void Reopen(string reason, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("O motivo da reabertura é obrigatório.");

            if (Status is not (TicketStatus.Resolvido or TicketStatus.Fechado))
                throw new DomainException("Só reabre chamado se estiver Resolvido ou Fechado.");

            Status = TicketStatus.EmAnalise;
            ClosedAt = null;
            SlaStartAt = now;
            SlaDueAt = CalculateSlaDueAt(Priority, now);

            AddComment(
                RequesterId,
                CommentVisibility.Internal,
                $"Chamado reaberto: {reason.Trim()}",
                now);
        }

        public void RaiseCreatedEvent(int requesterId, string requesterName, DateTime now)
        {
            Raise(TicketCreatedDomainEvent.Create(
                ticketId: Id,
                requesterId: requesterId,
                requesterName: requesterName,
                occurredAt: now));
        }

        public void RaiseAssignedEvent(int agentId, string agentName, string performedByUserName, DateTime now)
        {
            Raise(TicketAssignedDomainEvent.Create(
                ticketId: Id,
                agentId: agentId,
                agentName: agentName,
                performedByUserName: performedByUserName,
                occurredAt: now));
        }

        public void RaiseRequesterChangedEvent(
            int requesterId,
            string requesterName,
            string performedByUserName,
            string? requesterEmail,
            DateTime now)
        {
            Raise(TicketRequesterChangedDomainEvent.Create(
                ticketId: Id,
                requesterId: requesterId,
                requesterName: requesterName,
                performedByUserName: performedByUserName,
                requesterEmail: requesterEmail,
                occurredAt: now));
        }

        public void RaiseStatusChangedEvent(
            string previousStatus,
            string newStatus,
            string performedByUserName,
            DateTime now)
        {
            Raise(TicketStatusChangedDomainEvent.Create(
                ticketId: Id,
                previousStatus: previousStatus,
                newStatus: newStatus,
                performedByUserName: performedByUserName,
                occurredAt: now));
        }

        public void RaiseCanceledEvent(int actorUserId, string actorUserName, string reason, DateTime now)
        {
            Raise(TicketCanceledDomainEvent.Create(
                ticketId: Id,
                actorUserId: actorUserId,
                actorUserName: actorUserName,
                reason: reason,
                occurredAt: now));
        }

        public void RaiseReopenedEvent(int actorUserId, string actorUserName, string reason, DateTime now)
        {
            Raise(TicketReopenedDomainEvent.Create(
                ticketId: Id,
                actorUserId: actorUserId,
                actorUserName: actorUserName,
                reason: reason,
                occurredAt: now));
        }

        public void RaiseUpdatedEvent(IReadOnlyList<TicketUpdatedChange> changes, DateTime now)
        {
            if (changes.Count == 0)
                return;

            Raise(TicketUpdatedDomainEvent.Create(
                ticketId: Id,
                changes: changes,
                occurredAt: now));
        }

        private void EnsureIsActiveForEditing()
        {
            if (Status is TicketStatus.Fechado or TicketStatus.Cancelado)
                throw new DomainException("Não é possível editar chamados inativos.");
        }

        private void EnsureIsActiveForAssignment()
        {
            if (Status is TicketStatus.Fechado or TicketStatus.Cancelado)
                throw new DomainException("Não é possível atribuir chamados inativos.");
        }

        private void EnsureIsActiveForRequest()
        {
            if (Status is TicketStatus.Fechado or TicketStatus.Cancelado)
                throw new DomainException("Não é possível alterar o requester de chamados inativos.");
        }

        private void AddComment(int authorId, string visibility, string message, DateTime now)
        {
            var comment = TicketComment.CreateNew(
                Id,
                authorId,
                visibility,
                CommentMessage.Create(message),
                now);

            _comments.Add(comment);
        }

        private static DateTime? CalculateSlaDueAt(string priority, DateTime now)
        {
            TicketPriority.EnsureValid(priority);

            return priority switch
            {
                TicketPriority.Baixa => now.AddHours(72),
                TicketPriority.Media => now.AddHours(48),
                TicketPriority.Alta => now.AddHours(24),
                TicketPriority.Critica => now.AddHours(8),
                _ => null
            };
        }
    }
}