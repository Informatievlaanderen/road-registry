namespace RoadRegistry.BackOffice.Api.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using FluentValidation;
using FluentValidation.Results;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class ExtractsController
{
    private const string CloseRoute = "{downloadId}/close";

    [HttpPut(CloseRoute, Name = nameof(Close))]
    [SwaggerOperation(OperationId = nameof(Close), Description = "")]
    public async Task<IActionResult> Close(
        [FromRoute] string downloadId,
        [FromBody] CloseRequestBody requestBody,
        [FromServices] ExtractUploadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!DownloadId.CanParse(downloadId)) throw new DownloadExtractNotFoundException();

            if (Enum.TryParse<RoadNetworkExtractCloseReason>(requestBody.Reason, true, out var reason))
            {
                var request = new CloseRoadNetworkExtractRequest(DownloadId.Parse(downloadId), reason);
                await _mediator.Send(request, cancellationToken);

                return Accepted();
            }
            else
            {
                throw new ValidationException("OngeldigeReden", new[]
                {
                    new ValidationFailure(nameof(requestBody.Reason), "Opgegeven reden is niet geldig.")
                });
            }
        }
        catch (UploadExtractNotFoundException exception)
        {
            return NotFound();
        }
    }
}

public sealed record CloseRequestBody
{
    public string Reason { get; set; }
}
