using HelpDesk.Api.Contracts.ServiceCatalog.Requests;
using HelpDesk.Api.Contracts.ServiceCatalog.Responses;
using HelpDesk.Api.Mapping.ServiceCatalog;
using HelpDesk.Application.ServiceCatalog.UseCases.CreateCategory;
using HelpDesk.Application.ServiceCatalog.UseCases.DeleteCategory;
using HelpDesk.Application.ServiceCatalog.UseCases.GetCategoryById;
using HelpDesk.Application.ServiceCatalog.UseCases.ListCategories;
using HelpDesk.Application.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.ServiceCatalog;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly CreateCategoryHandler _create;
    private readonly GetCategoryByIdHandler _getById;
    private readonly ListCategoriesHandler _list;
    private readonly DeleteCategoryHandler _delete;

    public CategoriesController(
        CreateCategoryHandler create,
        GetCategoryByIdHandler getById,
        ListCategoriesHandler list,
        DeleteCategoryHandler delete)
    {
        _create = create;
        _getById = getById;
        _list = list;
        _delete = delete;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryItemResponse>> Create(
        [FromHeader(Name = "userId")] int userId,
        [FromBody] CreateCategoryRequest request)
    {
        try
        {
            var created = await _create.HandleAsync(
                new CreateCategoryCommand(userId, request.ToApplicationDto()));

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryItemResponse>> GetById([FromRoute] int id)
    {
        try
        {
            var category = await _getById.HandleAsync(new GetCategoryByIdQuery(id));
            return Ok(category.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CategoryItemResponse>>> List(
        [FromQuery] string? nameContains,
        [FromQuery] int? parentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var items = await _list.HandleAsync(
                new ListCategoriesQuery(nameContains, parentId, page, pageSize));

            return Ok(items.Select(x => x.ToResponse()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId)
    {
        try
        {
            await _delete.HandleAsync(new DeleteCategoryCommand(userId, id));
            return Ok(new { message = "Categoria excluída com sucesso!" });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }
}