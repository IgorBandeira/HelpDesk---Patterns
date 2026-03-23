using HelpDesk.Application.IdentityAccess.Ports;
using HelpDesk.Application.ServiceCatalog.DTOs;
using HelpDesk.Application.ServiceCatalog.Ports;
using HelpDesk.Application.Shared.Authorization;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Domain.ServiceCatalog.Aggregates;
using HelpDesk.Domain.ServiceCatalog.ValueObjects;
using HelpDesk.Domain.SharedKernel.Exceptions;

namespace HelpDesk.Application.ServiceCatalog.UseCases.CreateCategory
{
    public sealed class CreateCategoryHandler
    {
        private readonly IUserReadPort _userRead;
        private readonly ICategoryRepository _categories;

        public CreateCategoryHandler(IUserReadPort userRead, ICategoryRepository categories)
        {
            _userRead = userRead;
            _categories = categories;
        }

        public async Task<CategoryItemDto> HandleAsync(CreateCategoryCommand command)
        {
            if (command.UserId <= 0)
                throw new AppException(HttpStatusCodes.Unauthorized, "Usuário inválido ou não informado.");

            var authUser = await _userRead.GetByIdAsync(command.UserId);
            AuthorizationRules.EnsureManager(authUser, "Apenas Managers podem criar categorias.");

            CategoryName categoryName;
            try
            {
                categoryName = CategoryName.Create(command.Dto.Name);
            }
            catch (DomainException ex)
            {
                throw new AppException(HttpStatusCodes.BadRequest, ex.Message);
            }

            var exists = await _categories.NameExistsAsync(categoryName.Value);
            if (exists)
                throw new AppException(HttpStatusCodes.Conflict, "Já existe categoria com esse nome.");

            if (command.Dto.ParentId.HasValue)
            {
                var parentId = command.Dto.ParentId.Value;

                var parent = await _categories.GetByIdNoTrackingAsync(parentId);
                if (parent is null)
                    throw new AppException(HttpStatusCodes.BadRequest, "Categoria pai inexistente.");

                if (parent.ParentId.HasValue)
                    throw new AppException(HttpStatusCodes.Conflict, "Categorias têm no máximo dois níveis. O pai informado já é uma subcategoria.");
            }

            var category = Category.CreateNew(categoryName, command.Dto.ParentId);
            await _categories.AddAsync(category);

            return new CategoryItemDto(category.Id, category.Name.Value, category.ParentId);
        }
    }
}