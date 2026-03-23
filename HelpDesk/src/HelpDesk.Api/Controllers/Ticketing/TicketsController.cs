using HelpDesk.Api.Contracts.Ticketing.Requests;
using HelpDesk.Api.Contracts.Ticketing.Responses;
using HelpDesk.Api.Mapping.Ticketing;
using HelpDesk.Application.Shared.Errors;
using HelpDesk.Application.Ticketing.UseCases.AssignTicket;
using HelpDesk.Application.Ticketing.UseCases.CancelTicket;
using HelpDesk.Application.Ticketing.UseCases.ChangeRequester;
using HelpDesk.Application.Ticketing.UseCases.ChangeStatus;
using HelpDesk.Application.Ticketing.UseCases.CreateTicket;
using HelpDesk.Application.Ticketing.UseCases.GetTicketById;
using HelpDesk.Application.Ticketing.UseCases.ListTickets;
using HelpDesk.Application.Ticketing.UseCases.ReopenTicket;
using HelpDesk.Application.Ticketing.UseCases.UpdateTicket;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.Ticketing;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase
{
    private readonly CreateTicketHandler _create;
    private readonly GetTicketByIdHandler _getById;
    private readonly ListTicketsHandler _list;
    private readonly UpdateTicketHandler _update;
    private readonly AssignTicketHandler _assign;
    private readonly ChangeRequesterHandler _changeRequester;
    private readonly ChangeStatusHandler _changeStatus;
    private readonly ReopenTicketHandler _reopen;
    private readonly CancelTicketHandler _cancel;

    public TicketsController(
        CreateTicketHandler create,
        GetTicketByIdHandler getById,
        ListTicketsHandler list,
        UpdateTicketHandler update,
        AssignTicketHandler assign,
        ChangeRequesterHandler changeRequester,
        ChangeStatusHandler changeStatus,
        ReopenTicketHandler reopen,
        CancelTicketHandler cancel)
    {
        _create = create;
        _getById = getById;
        _list = list;
        _update = update;
        _assign = assign;
        _changeRequester = changeRequester;
        _changeStatus = changeStatus;
        _reopen = reopen;
        _cancel = cancel;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TicketResponse>> Create(
        [FromHeader(Name = "userId")] int userId,
        [FromBody] CreateTicketRequest request)
    {
        try
        {
            var created = await _create.HandleAsync(new CreateTicketCommand(userId, request.ToDto()));
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailsResponse>> GetById(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId)
    {
        try
        {
            var item = await _getById.HandleAsync(new GetTicketByIdQuery(id, userId));
            return Ok(item.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TicketListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TicketListItemResponse>>> List(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? title,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        [FromQuery] int? requesterId,
        [FromQuery] int? assigneeId,
        [FromQuery] int? categoryId,
        [FromQuery] DateTime? slaDueFrom,
        [FromQuery] DateTime? slaDueTo,
        [FromQuery] bool overdueOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var items = await _list.HandleAsync(new ListTicketsQuery(
                status, priority, title, createdFrom, createdTo,
                requesterId, assigneeId, categoryId,
                slaDueFrom, slaDueTo, overdueOnly, page, pageSize));

            return Ok(items.Select(x => x.ToResponse()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> Update(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId,
        [FromBody] UpdateTicketRequest request)
    {
        try
        {
            var updated = await _update.HandleAsync(new UpdateTicketCommand(id, userId, request.ToDto()));
            return Ok(updated.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPatch("{id:int}/assign")]
    [ProducesResponseType(typeof(AssignTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignTicketResponse>> Assign(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId,
        [FromBody] AssignTicketRequest request)
    {
        try
        {
            var result = await _assign.HandleAsync(new AssignTicketCommand(id, userId, request.ToDto()));
            return Ok(result.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPatch("{id:int}/requester")]
    [ProducesResponseType(typeof(RequesterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequesterResponse>> ChangeRequester(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId,
        [FromBody] ChangeRequesterRequest request)
    {
        try
        {
            var result = await _changeRequester.HandleAsync(new ChangeRequesterCommand(id, userId, request.ToDto()));
            return Ok(result.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ChangeStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangeStatusResponse>> ChangeStatus(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId,
        [FromBody] ChangeStatusRequest request)
    {
        try
        {
            var result = await _changeStatus.HandleAsync(new ChangeStatusCommand(id, userId, request.ToDto()));
            return Ok(result.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPatch("{id:int}/reopen")]
    [ProducesResponseType(typeof(ReopenTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReopenTicketResponse>> Reopen(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId,
        [FromBody] ReopenTicketRequest request)
    {
        try
        {
            var result = await _reopen.HandleAsync(new ReopenTicketCommand(id, userId, request.ToDto()));
            return Ok(result.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType(typeof(CancelTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CancelTicketResponse>> Cancel(
        [FromRoute] int id,
        [FromHeader(Name = "userId")] int userId,
        [FromBody] CancelTicketRequest request)
    {
        try
        {
            var result = await _cancel.HandleAsync(new CancelTicketCommand(id, userId, request.ToDto()));
            return Ok(result.ToResponse());
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }
}