using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.Ticketing.Enums;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Internal;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.Ticketing.Events;
using HelpDesk.Domain.Ticketing.ValueObjects;

namespace HelpDesk.Application.Ticketing.UseCases.UpdateTicket
{
    public sealed class UpdateTicketHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly ICategoryReadPort _categories;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public UpdateTicketHandler(
            ITicketRepository tickets,
            IUserReadPort users,
            ICategoryReadPort categories,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            => (_tickets, _users, _categories, _clock, _domainEventDispatcher) =
               (tickets, users, categories, clock, domainEventDispatcher);

        public async Task<TicketResponseDto> HandleAsync(UpdateTicketCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var t = await _tickets.GetByIdAsync(cmd.Id);
            if (t is null)
                throw new AppException(HttpStatusCodes.NotFound, "Chamado não encontrado.");

            if (t.Status is TicketStatus.Fechado or TicketStatus.Cancelado)
                throw new AppException(HttpStatusCodes.BadRequest, "Não é possível editar chamados inativos.");

            if (!TicketAuthRules.Owner(user, t))
                throw new AppException(HttpStatusCodes.Forbidden,
                    "Somente o solicitante do chamado ou um Manager pode editar este chamado.");

            TicketTitle? newTitle = null;
            TicketDescription? newDesc = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(cmd.Dto.Title))
                    newTitle = TicketTitle.Create(cmd.Dto.Title);

                if (!string.IsNullOrWhiteSpace(cmd.Dto.Description))
                    newDesc = TicketDescription.Create(cmd.Dto.Description);

                if (!string.IsNullOrWhiteSpace(cmd.Dto.Priority))
                    TicketPriority.EnsureValid(cmd.Dto.Priority);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            string? oldCategoryName = null;
            string? newCategoryName = null;

            if (cmd.Dto.CategoryId.HasValue)
            {
                var exists = await _categories.ExistsAsync(cmd.Dto.CategoryId.Value);
                if (!exists)
                    throw new AppException(HttpStatusCodes.BadRequest,
                        $"Categoria #{cmd.Dto.CategoryId.Value} não encontrada.");

                oldCategoryName = await _categories.GetNameAsync(t.CategoryId);
                newCategoryName = await _categories.GetNameAsync(cmd.Dto.CategoryId.Value);
            }

            var previousTitle = t.Title.Value;
            var previousDescription = t.Description.Value;
            var previousPriority = t.Priority;
            var previousCategoryId = t.CategoryId;

            var now = _clock.Now;

            bool changed;
            try
            {
                changed = t.Update(
                    newTitle,
                    newDesc,
                    cmd.Dto.Priority,
                    cmd.Dto.CategoryId,
                    now);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            if (!changed)
                throw new AppException(HttpStatusCodes.BadRequest, "Nenhuma alteração detectada.");

            var actions = TicketUpdateActionBuilder.Build(
                userName: user.Name,
                previousTitle: previousTitle,
                currentTitle: t.Title.Value,
                previousDescription: previousDescription,
                currentDescription: t.Description.Value,
                previousPriority: previousPriority,
                currentPriority: t.Priority,
                previousCategoryId: previousCategoryId,
                currentCategoryId: t.CategoryId,
                oldCategoryName: oldCategoryName,
                newCategoryName: newCategoryName);

            var changes = actions
                .Select(x => new TicketUpdatedChange(x))
                .ToList();

            t.RaiseUpdatedEvent(changes, now);

            await _tickets.SaveAsync(t);

            await _domainEventDispatcher.DispatchAsync(t.DomainEvents, ct);
            t.ClearDomainEvents();

            return new TicketResponseDto(
                t.Id,
                t.Title.Value,
                t.Description.Value,
                t.Status,
                t.Priority,
                t.CreatedAt,
                t.SlaStartAt,
                t.SlaDueAt,
                t.RequesterId,
                t.CategoryId
            );
        }
    }
}