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
    [ProducesResponseType(typeof(GetExtractDownloadPreSignedUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(DownloadExtract))]
    [HttpGet("{downloadId}/download", Name = nameof(DownloadExtract))]
    public async Task<IActionResult> DownloadExtract(
        [FromRoute] string downloadId,
        CancellationToken cancellationToken)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        try
        {
            var request = new GetDownloadExtractPreSignedUrlRequest(parsedDownloadId);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new GetExtractDownloadPreSignedUrlResponse
            {
                DownloadUrl = response.PreSignedUrl
            });
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
        catch (ExtractArchiveNotCreatedException)
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (BlobNotFoundException) // This condition can only occur if the blob no longer exists in the bucket
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
    }

    public class GetExtractDownloadPreSignedUrlResponse
    {
        public string DownloadUrl { get; init; }
    }
}
