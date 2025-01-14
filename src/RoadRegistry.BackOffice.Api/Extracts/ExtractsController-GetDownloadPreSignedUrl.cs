namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    private const string GetDownloadPreSignedUrlRoute = "download/{downloadId}/presignedurl";

    /// <summary>
    ///     Gets the pre-signed url to download the extract.
    /// </summary>
    /// <param name="downloadId">The download identifier.</param>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [HttpGet(GetDownloadPreSignedUrlRoute, Name = nameof(GetDownloadPreSignedUrlForExtract))]
    [SwaggerOperation(OperationId = nameof(GetDownloadPreSignedUrlForExtract), Description = "")]
    public async Task<ActionResult> GetDownloadPreSignedUrlForExtract(
        [FromRoute] string downloadId,
        [FromServices] ExtractDownloadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetDownloadFilePreSignedUrlRequest(downloadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(new GetExtractDownloadPreSignedUrlResponse
            {
                DownloadUrl = response.PreSignedUrl
            });
        }
        catch (ExtractArchiveNotCreatedException)
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (BlobNotFoundException) // This condition can only occur if the blob no longer exists in the bucket
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (DownloadExtractNotFoundException exception)
        {
            AddHeaderRetryAfter(exception.RetryAfterSeconds);
            return NotFound();
        }
        catch (ExtractDownloadNotFoundException exception)
        {
            AddHeaderRetryAfter(exception.RetryAfterSeconds);
            return NotFound();
        }
    }

    public class GetExtractDownloadPreSignedUrlResponse
    {
        public string DownloadUrl { get; init; }
    }
}
