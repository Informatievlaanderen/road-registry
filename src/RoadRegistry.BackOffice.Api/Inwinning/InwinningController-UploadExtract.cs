namespace RoadRegistry.BackOffice.Api.Inwinning;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Jobs;
using RoadRegistry.Extracts.Schema;
using Swashbuckle.AspNetCore.Annotations;
using TicketingService.Abstractions;

public partial class InwinningController
{
    /// <summary>
    ///     Vraag een pre-signed url aan voor een zip van een extract download te uploaden.
    /// </summary>
    /// <param name="downloadId"></param>
    /// <param name="extractsDbContext"></param>
    /// <param name="ticketing"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de url is aangemaakt.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [ProducesResponseType(typeof(UploadExtractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(UploadExtract))]
    [HttpPost("{downloadId}/upload", Name = nameof(UploadExtract))]
    public async Task<IActionResult> UploadExtract(
        [FromRoute] string downloadId,
        [FromServices] ExtractsDbContext extractsDbContext,
        [FromServices] ITicketing ticketing,
        CancellationToken cancellationToken = default)
    {
        //TODO-pr implement inwinning upload
        throw new NotImplementedException();
        // if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        // {
        //     throw new InvalidGuidValidationException("DownloadId");
        // }
        //
        // var record = await extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == parsedDownloadId.ToGuid(), cancellationToken);
        // if (record is null)
        // {
        //     return NotFound();
        // }
        //
        // if (record.TicketId is not null)
        // {
        //     var ticket = await ticketing.Get(record.TicketId.Value, cancellationToken);
        //     if (ticket is not null && ticket.Status != TicketStatus.Complete && ticket.Status != TicketStatus.Error)
        //     {
        //         throw new ValidationException([
        //             new ValidationFailure
        //             {
        //                 PropertyName = string.Empty,
        //                 ErrorCode = "UploadNietAfgerond",
        //                 ErrorMessage = "Er is nog een upload bezig voor dit extract.",
        //             }
        //         ]);
        //     }
        // }
        //
        // var response = await _mediator.Send(GetPresignedUploadUrlRequest.ForInwinning(parsedDownloadId), cancellationToken);
        // cancellationToken.ThrowIfCancellationRequested();
        //
        // record.TicketId = response.TicketId;
        // await extractsDbContext.SaveChangesAsync(cancellationToken);
        //
        // return Ok(new UploadExtractResponse(response.UploadUrl, response.UploadUrlFormData, response.TicketUrl));
    }

    public sealed record UploadExtractResponse(string UploadUrl, Dictionary<string, string> UploadUrlFormData, string TicketUrl);
}
