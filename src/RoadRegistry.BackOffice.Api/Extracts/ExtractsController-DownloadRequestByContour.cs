namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    private const string PostDownloadRequestByContourRoute = "downloadrequests/bycontour";

    /// <summary>
    ///     Requests the download by contour.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [HttpPost(PostDownloadRequestByContourRoute, Name = nameof(RequestDownloadByContour))]
    [SwaggerOperation(OperationId = nameof(RequestDownloadByContour), Description = "")]
    public async Task<ActionResult> RequestDownloadByContour([FromBody] DownloadExtractByContourRequestBody body, CancellationToken cancellationToken)
    {
        var request = new DownloadExtractByContourRequest(body.Contour, body.Buffer, body.Description, body.IsInformative);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody(response.DownloadId, response.IsInformative));
    }
}

public record DownloadExtractByContourRequestBody(int Buffer, string Contour, string Description, bool IsInformative);
