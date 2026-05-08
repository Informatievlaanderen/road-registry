namespace RoadRegistry.BackOffice.Api.Inwinning;

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

public partial class InwinningController
{
    /// <summary>
    ///     Vraag een pre-signed url aan voor een zip van een extract download te uploaden.
    /// </summary>
    /// <param name="downloadId"></param>
    /// <param name="dryRun"></param>
    /// <param name="extractsDbContext"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als de url is aangemaakt.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [ProducesResponseType(typeof(UploadInwinningExtractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(UploadInwinningExtract))]
    [HttpPost("{downloadId}/upload", Name = nameof(UploadInwinningExtract))]
    public async Task<IActionResult> UploadInwinningExtract(
        [FromRoute] string downloadId,
        [FromServices] ExtractsDbContext extractsDbContext,
        [FromQuery] bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        var extractDownload = await extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == parsedDownloadId.ToGuid(), cancellationToken);
        if (extractDownload is null)
        {
            return NotFound();
        }

        if (extractDownload.Closed)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = string.Empty,
                    ErrorCode = "ExtractGesloten",
                    ErrorMessage = "Het extract is gesloten.",
                }
            ]);
        }

        if (extractDownload.LatestUploadId is not null)
        {
            var extractUpload = await extractsDbContext.ExtractUploads.SingleAsync(x => x.UploadId == extractDownload.LatestUploadId.Value, cancellationToken);
            if (extractUpload.Status is ExtractUploadStatus.Processing or ExtractUploadStatus.AutomaticValidationSucceeded)
            {
                throw new ValidationException([
                    new ValidationFailure
                    {
                        PropertyName = string.Empty,
                        ErrorCode = "UploadNietAfgerond",
                        ErrorMessage = "Er is nog een upload bezig voor dit extract.",
                    }
                ]);
            }
        }

        var response = await _mediator.Send(GetPresignedUploadUrlRequest.ForInwinning(parsedDownloadId,
            dryRun: dryRun), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        extractDownload.TicketId = response.TicketId;
        extractDownload.LatestUploadId = null;
        await extractsDbContext.SaveChangesAsync(cancellationToken);

        return Ok(new UploadInwinningExtractResponse(response.UploadUrl, response.UploadUrlFormData, response.TicketUrl));
    }

    public sealed record UploadInwinningExtractResponse(string UploadUrl, Dictionary<string, string> UploadUrlFormData, string TicketUrl);
}
