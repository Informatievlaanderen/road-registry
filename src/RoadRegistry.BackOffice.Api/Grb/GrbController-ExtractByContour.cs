namespace RoadRegistry.BackOffice.Api.Grb;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class GrbController
{
    private const string PostDownloadRequestRoute = "extracts/bycontour";

    /// <summary>
    ///     Requests the download.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(PostDownloadRequestRoute, Name = nameof(ExtractByContour))]
    [SwaggerOperation(OperationId = nameof(ExtractByContour), Description = "")]
    public async Task<IActionResult> ExtractByContour([FromBody] DownloadExtractRequestBody body, CancellationToken cancellationToken)
    {
        var isInformative = body.IsInformative ??
                            body.RequestId?.StartsWith("INF_")
                            ?? false;

        var request = new DownloadExtractRequest(body.RequestId, body.Contour, isInformative);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody(response.DownloadId, response.IsInformative));
    }
}

public record DownloadExtractRequestBody(string Contour, string RequestId, bool? IsInformative);

public record DownloadExtractResponseBody(string DownloadId, bool IsInformative);
