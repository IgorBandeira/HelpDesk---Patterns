using HelpDesk.Domain.Collaboration.Aggregates;
using HelpDesk.Domain.Collaboration.Enums;
using HelpDesk.Domain.Collaboration.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Domain.Ticketing.ValueObjects;

namespace HelpDesk.Domain.Ticketing.Aggregates
{
    public sealed class Ticket
    {
        public int Id { get; private set; }
        public TicketTitle Title { get; private set; }
        public TicketDescription Description { get; private set; }
        public string Status { get; private set; }
        public string Priority { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime SlaStartAt { get; private set; }
        public DateTime? AssignedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public DateTime? SlaDueAt { get; private set; }
        public int RequesterId { get; private set; }
        public int? AssigneeId { get; private set; }
        public int CategoryId { get; private set; }

        private readonly List<TicketComment> _comments = new();
        public IReadOnlyCollection<TicketComment> Comments => _comments.AsReadOnly();

        private Ticket(
            TicketTitle title,
            TicketDescription description,
            string priority,
            int requesterId,
            int categoryId,
            DateTime now)
        {
            TicketPriority.EnsureValid(priority);

            Title = title;
            Description = description;
            Priority = priority;

            RequesterId = requesterId;
            CategoryId = categoryId;

            Status = TicketStatus.Novo;

            CreatedAt = now;
            SlaStartAt = now;
            SlaDueAt = now + TicketPriority.ToSla(priority);
        }

        public static Ticket CreateNew(
            TicketTitle title,
            TicketDescription description,
            string priority,
            int requesterId,
            int categoryId,
            DateTime now)
            => new(title, description, priority, requesterId, categoryId, now);

        public void EnsureActive()
        {
            if (Status is TicketStatus.Fechado or TicketStatus.Cancelado)
                throw new DomainException("Chamado inativo.");
        }

        public bool Update(
            TicketTitle? newTitle,
            TicketDescription? newDescription,
            string? newPriority,
            int? newCategoryId,
            DateTime now)
        {
            EnsureActive();

            var changed = false;

            if (newTitle != null && !string.Equals(Title.Value, newTitle.Value, StringComparison.Ordinal))
            {
                Title = newTitle;
                changed = true;
            }

            if (newDescription != null && !string.Equals(Description.Value, newDescription.Value, StringComparison.Ordinal))
            {
                Description = newDescription;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(newPriority))
            {
                TicketPriority.EnsureValid(newPriority);

                if (!string.Equals(Priority, newPriority, StringComparison.OrdinalIgnoreCase))
                {
                    Priority = newPriority;
                    SlaStartAt = now;
                    SlaDueAt = now + TicketPriority.ToSla(newPriority);
                    changed = true;
                }
            }

            if (newCategoryId.HasValue && newCategoryId.Value != CategoryId)
            {
                CategoryId = newCategoryId.Value;
                changed = true;
            }

            return changed;
        }

        public void AssignToAgent(int agentId, DateTime now)
        {
            EnsureActive();

            AssigneeId = agentId;
            AssignedAt = now;

            if (Status == TicketStatus.Novo)
                Status = TicketStatus.EmAnalise;
        }

        public void ChangeRequester(int requesterId)
        {
            EnsureActive();
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

        public void Reopen(string reason, DateTime now)
        {
            if (Status is not (TicketStatus.Resolvido or TicketStatus.Fechado))
                throw new DomainException("Só reabre chamado se estiver Resolvido ou Fechado.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("O motivo da reabertura é obrigatório.");

            Status = TicketStatus.EmAnalise;
            ClosedAt = null;

            SlaStartAt = now;
            SlaDueAt = now + TicketPriority.ToSla(Priority);

            AddComment(
                RequesterId,
                CommentVisibility.Internal,
                $"Chamado reaberto: {reason.Trim()}",
                now);
        }

        public void Cancel(int authorId, string reason, DateTime now)
        {
            if (Status is not TicketStatus.Novo and not TicketStatus.EmAnalise)
                throw new DomainException("Só cancela se Novo/Em Análise.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("O motivo do cancelamento é obrigatório.");

            Status = TicketStatus.Cancelado;
            ClosedAt = now;

            AddComment(
                authorId,
                CommentVisibility.Internal,
                $"Chamado cancelado: {reason.Trim()}",
                now);
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
    }
}