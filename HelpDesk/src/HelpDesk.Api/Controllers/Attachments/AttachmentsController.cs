using HelpDesk.Api.Contracts.Attachments.Responses;
using HelpDesk.Api.Mapping.Attachments;
using HelpDesk.Application.Attachments.Ports;
using HelpDesk.Application.Attachments.UseCases.DeleteAttachment;
using HelpDesk.Application.Attachments.UseCases.GetAttachmentById;
using HelpDesk.Application.Attachments.UseCases.ListAttachments;
using HelpDesk.Application.Attachments.UseCases.UploadAttachment;
using HelpDesk.Application.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.Attachments
{
    [ApiController]
    [Route("api/tickets/{ticketId:int}/attachments")]
    public sealed class AttachmentsController : ControllerBase
    {
        private readonly UploadAttachmentHandler _upload;
        private readonly ListAttachmentsHandler _list;
        private readonly GetAttachmentByIdHandler _getById;
        private readonly DeleteAttachmentHandler _delete;

        public AttachmentsController(
            UploadAttachmentHandler upload,
            ListAttachmentsHandler list,
            GetAttachmentByIdHandler getById,
            DeleteAttachmentHandler delete)
        {
            _upload = upload;
            _list = list;
            _getById = getById;
            _delete = delete;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AttachmentCreatedResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AttachmentCreatedResponse>> Upload(
            [FromRoute] int ticketId,
            [FromHeader(Name = "userId")] int userId,
            IFormFile file)
        {
            try
            {
                if (file is null)
                    return BadRequest("Arquivo não informado.");

                await using var stream = file.OpenReadStream();

                var uploadFile = new UploadFile(
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    stream);

                var created = await _upload.HandleAsync(
                    new UploadAttachmentCommand(ticketId, userId, uploadFile));

                return CreatedAtAction(
                    nameof(GetById),
                    new { ticketId = created.TicketId, attachmentId = created.Id },
                    created.ToCreatedResponse());
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AttachmentItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AttachmentItemResponse>>> List([FromRoute] int ticketId)
        {
            try
            {
                var items = await _list.HandleAsync(new ListAttachmentsQuery(ticketId));
                return Ok(items.ToResponseList());
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }

        [HttpGet("{attachmentId:int}")]
        [ProducesResponseType(typeof(AttachmentItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AttachmentItemResponse>> GetById(
            [FromRoute] int ticketId,
            [FromRoute] int attachmentId)
        {
            try
            {
                var item = await _getById.HandleAsync(
                    new GetAttachmentByIdQuery(ticketId, attachmentId));

                return Ok(item.ToResponse());
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }

        [HttpDelete("{attachmentId:int}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            [FromRoute] int ticketId,
            [FromRoute] int attachmentId,
            [FromHeader(Name = "userId")] int userId)
        {
            try
            {
                await _delete.HandleAsync(
                    new DeleteAttachmentCommand(ticketId, attachmentId, userId));

                return Ok(new { message = "Anexo excluído com sucesso!" });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
        }
    }
}