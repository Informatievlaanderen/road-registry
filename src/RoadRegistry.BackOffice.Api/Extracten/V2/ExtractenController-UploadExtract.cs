namespace RoadRegistry.BackOffice.Api.Extracten;

using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Jobs;
using RoadRegistry.Extracts.Schema;
using Swashbuckle.AspNetCore.Annotations;
using Version = Infrastructure.Version;

public partial class ExtractenController
{
    /// <summary>
    ///     Vraag een pre-signed url aan om een levering in het hernieuwde datamodel op te laden.
    /// </summary>
    [ProducesResponseType(typeof(UploadExtractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(UploadExtractV2))]
    [MapToApiVersion(Version.V2)]
    [HttpPost("{downloadId}/upload", Name = nameof(UploadExtractV2))]
    public async Task<IActionResult> UploadExtractV2(
        [FromRoute] string downloadId,
        [FromServices] ExtractsDbContext extractsDbContext,
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

        var response = await _mediator.Send(GetPresignedUploadUrlRequest.ForBijhouding(parsedDownloadId), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        extractDownload.LatestUploadId = null;
        await extractsDbContext.SaveChangesAsync(cancellationToken);

        return Ok(new UploadExtractResponse(response.UploadUrl, response.UploadUrlFormData, response.TicketUrl));
    }
}
