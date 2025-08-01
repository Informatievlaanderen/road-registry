namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extensions;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    [ProducesResponseType(typeof(List<Extracts.ExtractDetailsResponseBody>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetList))]
    [HttpGet("", Name = nameof(GetList))]
    public async Task<ActionResult> GetList(
        [FromServices] ExtractDownloadsOptions options,
        CancellationToken cancellationToken)
    {
        //TODO-pr implement GetList filtering by OrganizationCode (=OperatorName)
        throw new NotImplementedException();
        // try
        // {
        //     if (!DownloadId.TryParse(downloadId, out var parsedDownloadId))
        //     {
        //         throw new InvalidGuidValidationException("DownloadId");
        //     }
        //
        //     var request = new ExtractDetailsRequest(parsedDownloadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
        //     var response = await _mediator.Send(request, cancellationToken);
        //
        //     return Ok(new Extracts.ExtractDetailsResponseBody
        //     {
        //         DownloadId = response.DownloadId,
        //         Description = response.Description,
        //         Contour = response.Contour.ToGeoJson(),
        //         ExtractRequestId = response.ExtractRequestId,
        //         RequestedOn = response.RequestedOn,
        //         IsInformative = response.IsInformative,
        //         ArchiveId = response.ArchiveId,
        //         TicketId = response.TicketId,
        //         DownloadAvailable = response.DownloadAvailable,
        //         ExtractDownloadTimeoutOccurred = response.ExtractDownloadTimeoutOccurred,
        //     });
        // }
        // catch (ExtractArchiveNotCreatedException)
        // {
        //     return StatusCode((int)HttpStatusCode.Gone);
        // }
        // catch (BlobNotFoundException) // This condition can only occur if the blob no longer exists in the bucket
        // {
        //     return StatusCode((int)HttpStatusCode.Gone);
        // }
        // catch (DownloadExtractNotFoundException exception)
        // {
        //     AddHeaderRetryAfter(exception.RetryAfterSeconds);
        //     return NotFound();
        // }
        // catch (ExtractDownloadNotFoundException exception)
        // {
        //     AddHeaderRetryAfter(exception.RetryAfterSeconds);
        //     return NotFound();
        // }
    }
}
