namespace RoadRegistry.BackOffice.Api.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public partial class ExtractsController
{
    private const string GetDownloadRoute = "download/{downloadId}";

    /// <summary>
    ///     Gets the download.
    /// </summary>
    /// <param name="downloadId">The download identifier.</param>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [HttpGet(GetDownloadRoute, Name = nameof(GetDownload))]
    [SwaggerOperation(OperationId = nameof(GetDownload), Description = "")]
    public async Task<ActionResult> GetDownload(
        [FromRoute] string downloadId,
        [FromServices] ExtractDownloadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadFileContentRequest(downloadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
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
}
