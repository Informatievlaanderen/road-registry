namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    private const string PostDownloadRequestByNisCodeRoute = "downloadrequests/byniscode";

    /// <summary>
    ///     Requests the download by nis code.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [HttpPost(PostDownloadRequestByNisCodeRoute, Name = nameof(PostDownloadRequestByNisCodeRoute))]
    [SwaggerOperation(OperationId = nameof(RequestDownloadByNisCode), Description = "")]
    public async Task<ActionResult> RequestDownloadByNisCode([FromBody] DownloadExtractByNisCodeRequestBody body, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadExtractByNisCodeRequest(body.NisCode, body.Buffer, body.Description, body.IsInformative);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new DownloadExtractResponseBody(response.DownloadId, response.IsInformative));
        }
        catch (DownloadExtractByNisCodeNotFoundException)
        {
            return NotFound();
        }
    }
}

public record DownloadExtractByNisCodeRequestBody(int Buffer, string Description, string NisCode, bool IsInformative);
