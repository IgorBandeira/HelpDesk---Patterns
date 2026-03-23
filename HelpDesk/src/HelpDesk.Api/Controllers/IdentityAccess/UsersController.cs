using HelpDesk.Api.Contracts.IdentityAccess.Requests;
using HelpDesk.Api.Contracts.IdentityAccess.Responses;
using HelpDesk.Api.Mapping.IdentityAccess;
using HelpDesk.Application.IdentityAccess.DTOs;
using HelpDesk.Application.IdentityAccess.UseCases.CreateUser;
using HelpDesk.Application.IdentityAccess.UseCases.DeleteUser;
using HelpDesk.Application.IdentityAccess.UseCases.GetUserById;
using HelpDesk.Application.IdentityAccess.UseCases.ListUsers;
using HelpDesk.Application.IdentityAccess.UseCases.PatchUser;
using HelpDesk.Application.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.IdentityAccess;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly CreateUserHandler _create;
    private readonly GetUserByIdHandler _getById;
    private readonly ListUsersHandler _list;
    private readonly PatchUserHandler _patch;
    private readonly DeleteUserHandler _delete;

    public UsersController(
        CreateUserHandler create,
        GetUserByIdHandler getById,
        ListUsersHandler list,
        PatchUserHandler patch,
        DeleteUserHandler delete)
    {
        _create = create;
        _getById = getById;
        _list = list;
        _patch = patch;
        _delete = delete;
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(
        [FromHeader(Name = "userId")] int userId,
        [FromBody] CreateUserRequest request)
    {
        try
        {
            var dto = new CreateUserDto
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role ?? "Requester"
            };

            var created = await _create.HandleAsync(new CreateUserCommand(userId, dto));
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, UserApiMapping.ToResponse(created));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserWithTicketsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserWithTicketsResponse>> GetById([FromRoute] int id)
    {
        try
        {
            var user = await _getById.HandleAsync(new GetUserByIdQuery(id));
            return Ok(UserApiMapping.ToWithTicketsResponse(user));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> List(
        [FromQuery] string? role,
        [FromQuery] string? email,
        [FromQuery] string? name,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var items = await _list.HandleAsync(new ListUsersQuery(role, email, name, page, pageSize));
            return Ok(items.Select(UserApiMapping.ToResponse));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Patch(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId,
        [FromBody] UpdateUserRequest request)
    {
        try
        {
            var dto = new UpdateUserDto
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role
            };

            var updated = await _patch.HandleAsync(new PatchUserCommand(id, userId, dto));
            return Ok(UserApiMapping.ToResponse(updated));
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
            await _delete.HandleAsync(new DeleteUserCommand(id, userId));
            return Ok(new { message = "Usuário excluído com sucesso!" });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }
}