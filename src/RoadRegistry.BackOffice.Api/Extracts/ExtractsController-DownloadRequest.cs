namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    private const string PostDownloadRequestRoute = "downloadrequests";

    /// <summary>
    ///     Requests the download.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(PostDownloadRequestRoute, Name = nameof(RequestDownload))]
    [SwaggerOperation(OperationId = nameof(RequestDownload), Description = "")]
    public async Task<IActionResult> RequestDownload([FromBody] DownloadExtractRequestBody body, CancellationToken cancellationToken)
    {
        var request = new DownloadExtractRequest(body.RequestId, body.Contour, body.IsInformative);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody(response.DownloadId, response.IsInformative));
    }
}

public record DownloadExtractRequestBody(string Contour, string RequestId, bool IsInformative);

public record DownloadExtractResponseBody(string DownloadId, bool IsInformative);
