namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Extracts.V2;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Gets the pre-signed url to download the extract.
    /// </summary>
    /// <param name="downloadId">The download identifier.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(DownloadExtractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(DownloadExtract))]
    [HttpGet("{downloadId}/download", Name = nameof(DownloadExtract))]
    public async Task<IActionResult> DownloadExtract(
        [FromRoute] string downloadId,
        CancellationToken cancellationToken = default)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        try
        {
            var request = new GetDownloadExtractPresignedUrlRequest(parsedDownloadId);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new DownloadExtractResponse
            {
                DownloadUrl = response.PresignedUrl
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

    public class DownloadExtractResponse
    {
        public string DownloadUrl { get; init; }
    }
}
