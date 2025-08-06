namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Extracts.Schema;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Vraag een pre-signed url aan voor een zip van een extract download te uploaden.
    /// </summary>
    /// <param name="downloadId"></param>
    /// <param name="extractsDbContext"></param>
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
        CancellationToken cancellationToken)
    {
        if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        {
            throw new InvalidGuidValidationException("DownloadId");
        }

        var record = await extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == parsedDownloadId.ToGuid(), cancellationToken);
        if (record is null)
        {
            return NotFound();
        }

        //TODO-pr validaties wanneer er geen upload meer mag gebeuren, is dat nodig? de lambda gaat dat sowieso al aftoetsen
        //record.IsInformative
        //record.DownloadAvailable
        //record.ArchiveId
        //record.Closed
        //record.DownloadedOn

        var response = await _mediator.Send(GetPresignedUploadUrlRequest.ForExtractsV2(parsedDownloadId), cancellationToken);

        //TODO-pr mss dit via lambda doen? zodat er maar 1 instance per extract iets kan wijzigen, maakt het mss wel lastig
        record.TicketId = response.TicketId;
        await extractsDbContext.SaveChangesAsync(cancellationToken);

        return Ok(new UploadExtractResponse(response.UploadUrl, response.UploadUrlFormData, response.TicketUrl));
    }

    public sealed record UploadExtractResponse(string UploadUrl, Dictionary<string, string> UploadUrlFormData, string TicketUrl);
}
