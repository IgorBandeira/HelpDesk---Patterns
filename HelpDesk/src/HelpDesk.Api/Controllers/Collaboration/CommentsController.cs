using HelpDesk.Api.Contracts.Collaboration.Requests;
using HelpDesk.Api.Contracts.Collaboration.Responses;
using HelpDesk.Api.Mapping.Collaboration;
using HelpDesk.Application.Collaboration.UseCases.AddComment;
using HelpDesk.Application.Collaboration.UseCases.DeleteComment;
using HelpDesk.Application.Collaboration.UseCases.GetCommentById;
using HelpDesk.Application.Collaboration.UseCases.ListComments;
using HelpDesk.Application.Collaboration.UseCases.ReplaceCommentMessage;
using HelpDesk.Application.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.Collaboration
{
    [ApiController]
    [Route("api/tickets/{ticketId:int}/comments")]
    public sealed class CommentsController : ControllerBase
    {
        private readonly AddCommentHandler _add;
        private readonly GetCommentByIdHandler _getById;
        private readonly ListCommentsHandler _list;
        private readonly ReplaceCommentMessageHandler _replace;
        private readonly DeleteCommentHandler _delete;

        public CommentsController(
            AddCommentHandler add,
            GetCommentByIdHandler getById,
            ListCommentsHandler list,
            ReplaceCommentMessageHandler replace,
            DeleteCommentHandler delete)
        {
            _add = add;
            _getById = getById;
            _list = list;
            _replace = replace;
            _delete = delete;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentResponse>> Add(
            [FromRoute] int ticketId,
            [FromHeader(Name = "userId")] int userId,
            [FromBody] AddCommentRequest request)
        {
            try
            {
                var created = await _add.HandleAsync(
                    new AddCommentCommand(ticketId, userId, request.ToApplicationDto()));

                return CreatedAtAction(
                    nameof(GetById),
                    new { ticketId, commentId = created.Id },
                    created.ToResponse());
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }

        [HttpGet("{commentId:int}")]
        [ProducesResponseType(typeof(CommentDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDetailsResponse>> GetById(
            [FromRoute] int ticketId,
            [FromRoute] int commentId,
            [FromHeader(Name = "userId")] int userId)
        {
            try
            {
                var comment = await _getById.HandleAsync(
                    new GetCommentByIdQuery(ticketId, commentId, userId));

                return Ok(comment.ToDetailsResponse());
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CommentDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CommentDetailsResponse>>> List(
            [FromRoute] int ticketId,
            [FromHeader(Name = "userId")] int userId)
        {
            try
            {
                var items = await _list.HandleAsync(new ListCommentsQuery(ticketId, userId));
                return Ok(items.ToResponseList());
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }

        [HttpPut("{commentId:int}")]
        [ProducesResponseType(typeof(CommentDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDetailsResponse>> ReplaceMessage(
            [FromRoute] int ticketId,
            [FromRoute] int commentId,
            [FromHeader(Name = "userId")] int userId,
            [FromBody] ReplaceCommentMessageRequest request)
        {
            try
            {
                var updated = await _replace.HandleAsync(
                    new ReplaceCommentMessageCommand(ticketId, commentId, userId, request.ToApplicationDto()));

                return Ok(updated.ToDetailsResponse());
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }

        [HttpDelete("{commentId:int}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            [FromRoute] int ticketId,
            [FromRoute] int commentId,
            [FromHeader(Name = "userId")] int userId)
        {
            try
            {
                await _delete.HandleAsync(new DeleteCommentCommand(ticketId, commentId, userId));
                return Ok(new { message = "Comentário excluído com sucesso!" });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }
    }
}