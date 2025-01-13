namespace RoadRegistry.BackOffice.Api.Grb;

using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Jobs;
using Editor.Schema;
using Exceptions;

public partial class GrbController
{
    /// <summary>
    ///     Vraag een pre-signed url aan voor een zip van een extract download te uploaden.
    /// </summary>
    /// <param name="downloadId"></param>
    /// <param name="editorContext"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de url is aangemaakt.</response>
    /// <response code="400">Als de url is aangemaakt.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [ProducesResponseType(typeof(GetPresignedUploadUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(FileCallbackResultExamples))]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(UploadForDownload), Description = "")]
    [HttpPost("download/{downloadId}/upload", Name = nameof(UploadForDownload))]
    public async Task<IActionResult> UploadForDownload(
        [FromRoute] string downloadId,
        [FromServices] EditorContext editorContext,
        CancellationToken cancellationToken)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        var record = await editorContext.ExtractRequests.FindAsync(new object[] { parsedDownloadId.ToGuid() }, cancellationToken);
        if (record is null)
        {
            throw new ExtractRequestNotFoundException(parsedDownloadId);
        }

        return Ok(await _mediator.Send(GetPresignedUploadUrlRequest.ForExtracts(parsedDownloadId), cancellationToken));
    }
}
