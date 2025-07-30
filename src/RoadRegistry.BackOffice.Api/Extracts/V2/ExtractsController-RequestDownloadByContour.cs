namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Requests the download by contour.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(DownloadExtractResponseBody), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(RequestDownloadByContour))]
    [HttpPost("downloadrequests/bycontour", Name = nameof(RequestDownloadByContour))]
    public async Task<ActionResult> RequestDownloadByContour([FromBody] DownloadExtractByContourRequestBody body, CancellationToken cancellationToken)
    {
        var request = new DownloadExtractByContourRequest(body.Contour, body.Buffer, body.Description, body.IsInformative);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody(response.DownloadId, response.IsInformative));
    }
}

public record DownloadExtractByContourRequestBody(int Buffer, string Contour, string Description, bool IsInformative);
