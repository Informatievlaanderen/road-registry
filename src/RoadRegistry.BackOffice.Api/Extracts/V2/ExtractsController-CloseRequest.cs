namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts.V2;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Extracts.Schema;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(Close))]
    [HttpPost("{downloadId}/close", Name = nameof(Close))]
    public async Task<IActionResult> Close(
        [FromRoute] string downloadId,
        //[FromBody] CloseRequestBody requestBody,
        [FromServices] ExtractsDbContext extractsDbContext,
        CancellationToken cancellationToken)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        // if (!Enum.TryParse<RoadNetworkExtractCloseReason>(requestBody.Reason, true, out var reason))
        // {
        //     throw new ValidationException("OngeldigeReden", [
        //         new ValidationFailure(nameof(requestBody.Reason), "Opgegeven reden is niet geldig.")
        //     ]);
        // }

        var extractRequest = await extractsDbContext.ExtractRequests.SingleOrDefaultAsync(x => x.DownloadId == parsedDownloadId.ToGuid(), cancellationToken: cancellationToken);
        if (extractRequest is null)
        {
            return NotFound();
        }

        try
        {
            var result = await _mediator.Send(new CloseExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                Request = new CloseExtractRequest(extractRequest.ExtractRequestId)
            }, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

public sealed record CloseRequestBody
{
    public string Reason { get; set; }
}
