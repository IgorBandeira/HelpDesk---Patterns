using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Application.Shared.Authorization;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.Ports;

namespace HelpDesk.Application.ServiceCatalog.UseCases.DeleteCategory
{
    public sealed class DeleteCategoryHandler
    {
        private readonly IUserReadPort _userRead;
        private readonly ICategoryRepository _categories;
        private readonly ITicketCategoryQueryPort _tickets;

        public DeleteCategoryHandler(
            IUserReadPort userRead,
            ICategoryRepository categories,
            ITicketCategoryQueryPort tickets)
        {
            _userRead = userRead;
            _categories = categories;
            _tickets = tickets;
        }

        public async Task HandleAsync(DeleteCategoryCommand command)
        {
            if (command.UserId <= 0)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var authUser = await _userRead.GetByIdAsync(command.UserId);
            AuthorizationRules.EnsureManager(authUser, "Apenas Managers podem deletar categorias.");

            var category = await _categories.GetByIdAsync(command.CategoryId);
            if (category is null)
                throw new AppException(HttpStatusCodes.NotFound, "Categoria não encontrada.");

            var hasChildren = await _categories.HasChildrenAsync(command.CategoryId);
            if (hasChildren)
                throw new AppException(HttpStatusCodes.Conflict, "Categoria possui subcategorias (filhos).");

            var hasActiveTickets = await _tickets.HasActiveTicketsForCategoryAsync(command.CategoryId);
            if (hasActiveTickets)
                throw new AppException(HttpStatusCodes.Conflict, "Categoria está associada a chamados ativos.");

            await _categories.DeleteAsync(category);
        }
    }
}