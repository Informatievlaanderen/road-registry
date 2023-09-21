namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    private const string GetStatusRoute = "upload/{uploadId}/status";

    /// <summary>
    ///     Gets the status.
    /// </summary>
    /// <param name="uploadId">The upload identifier.</param>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpGet(GetStatusRoute, Name = nameof(GetStatus))]
    [SwaggerOperation(OperationId = nameof(GetStatus), Description = "")]
    public async Task<IActionResult> GetStatus(
        [FromRoute] string uploadId,
        [FromServices] ExtractUploadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new UploadStatusRequest(uploadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
            var response = await _mediator.Send(request, cancellationToken);
            AddHeaderRetryAfter(response.RetryAfter);
            return Ok(new GetStatusResponseBody(response.Status));
        }
        catch (UploadExtractNotFoundException exception)
        {
            AddHeaderRetryAfter(exception.RetryAfterSeconds);
            return NotFound();
        }
    }
}

public record GetStatusResponseBody(string Status);
