namespace RoadRegistry.BackOffice.Api.Extracten;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractenController
{
    /// <summary>
    ///     Gets the pre-signed url to download the uploaded extract.
    /// </summary>
    /// <param name="downloadId">The download identifier.</param>
    /// <param name="cancellationToken"></param>
    [ProducesResponseType(typeof(DownloadUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(DownloadUpload))]
    [HttpGet("{downloadId}/upload", Name = nameof(DownloadUpload))]
    public async Task<IActionResult> DownloadUpload(
        [FromRoute] string downloadId,
        CancellationToken cancellationToken = default)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        try
        {
            var request = new GetDownloadUploadPresignedUrlRequest(parsedDownloadId);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new DownloadUploadResponse
            {
                DownloadUrl = response.PresignedUrl,
                FileName = response.FileName
            });
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
        catch (BlobNotFoundException)
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
    }

    public class DownloadUploadResponse
    {
        public string DownloadUrl { get; init; }
        public string FileName { get; init; }
    }
}
