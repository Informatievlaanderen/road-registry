namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Jobs;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Editor.Schema;
using Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class ExtractsController
{
    /// <summary>
    ///     Vraag een pre-signed url aan voor een zip van een extract download te uploaden.
    /// </summary>
    /// <param name="downloadId"></param>
    /// <param name="editorContext"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de url is aangemaakt.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [ProducesResponseType(typeof(GetPresignedUploadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(UploadExtract))]
    [HttpPost("{downloadId}/upload", Name = nameof(UploadExtract))]
    public async Task<IActionResult> UploadExtract(
        [FromRoute] string downloadId,
        [FromServices] EditorContext editorContext,
        CancellationToken cancellationToken)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        var record = await editorContext.ExtractRequests.FindAsync([parsedDownloadId.ToGuid()], cancellationToken);
        if (record is null)
        {
            throw new ExtractRequestNotFoundException(parsedDownloadId);
        }

        return Ok(await _mediator.Send(GetPresignedUploadUrlRequest.ForExtracts(parsedDownloadId), cancellationToken));
    }
}
