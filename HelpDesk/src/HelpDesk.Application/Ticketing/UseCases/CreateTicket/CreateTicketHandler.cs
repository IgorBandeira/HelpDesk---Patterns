using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Application.Shared.Abstractions;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.DTOs;
using HelpDesk.Application.Ticketing.Internal;
using HelpDesk.Application.Ticketing.Ports;
using HelpDesk.Domain.SharedKernel.Exceptions;
using HelpDesk.Domain.Ticketing.Aggregates;
using HelpDesk.Domain.Ticketing.ValueObjects;

namespace HelpDesk.Application.Ticketing.UseCases.CreateTicket
{
    public sealed class CreateTicketHandler
    {
        private readonly ITicketRepository _tickets;
        private readonly IUserReadPort _users;
        private readonly ICategoryReadPort _categories;
        private readonly IClock _clock;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public CreateTicketHandler(
            ITicketRepository tickets,
            IUserReadPort users,
            ICategoryReadPort categories,
            IClock clock,
            IDomainEventDispatcher domainEventDispatcher)
            => (_tickets, _users, _categories, _clock, _domainEventDispatcher) =
               (tickets, users, categories, clock, domainEventDispatcher);

        public async Task<TicketResponseDto> HandleAsync(CreateTicketCommand cmd, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(cmd.UserId);
            if (user is null)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            if (!(TicketAuthRules.IsManager(user) || TicketAuthRules.IsRequester(user)))
                throw new AppException(HttpStatusCodes.Forbidden, "Somente Requester ou Manager podem criar chamados.");

            TicketTitle title;
            TicketDescription desc;

            try
            {
                Domain.Ticketing.Enums.TicketPriority.EnsureValid(cmd.Dto.Priority);
                title = TicketTitle.Create(cmd.Dto.Title);
                desc = TicketDescription.Create(cmd.Dto.Description);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            var catExists = await _categories.ExistsAsync(cmd.Dto.CategoryId);
            if (!catExists)
                throw new AppException(HttpStatusCodes.BadRequest, $"Categoria #{cmd.Dto.CategoryId} não encontrada.");

            var now = _clock.Now;

            var t = Ticket.CreateNew(
                title,
                desc,
                cmd.Dto.Priority,
                requesterId: user.Id,
                categoryId: cmd.Dto.CategoryId,
                now: now);

            await _tickets.AddAsync(t);

            t.RaiseCreatedEvent(user.Id, user.Name, now);

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