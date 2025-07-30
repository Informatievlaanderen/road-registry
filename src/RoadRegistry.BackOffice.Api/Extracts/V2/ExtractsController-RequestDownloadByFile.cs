namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using RoadRegistry.BackOffice.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Requests the download by file.
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
    [SwaggerOperation(OperationId = nameof(RequestDownloadByFile))]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    [HttpPost("downloadrequests/byfile", Name = nameof(RequestDownloadByFile))]
    public async Task<ActionResult> RequestDownloadByFile(DownloadExtractByFileRequestBody body, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadExtractByFileRequest(BuildRequestItem(".shp"), BuildRequestItem(".prj"), body.Buffer, body.Description, body.IsInformative);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new DownloadExtractResponseBody(response.DownloadId, response.IsInformative));

            DownloadExtractByFileRequestItem BuildRequestItem(string extension)
            {
                var file = body.Files?.SingleOrDefault(formFile => formFile.FileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
                           ?? throw new DutchValidationException([new ValidationFailure(nameof(body.Files), $"Een bestand met de extensie '{extension}' ontbreekt.")]);
                var fileStream = new MemoryStream();
                file.CopyTo(fileStream);
                fileStream.Position = 0;
                return new DownloadExtractByFileRequestItem(file.FileName, fileStream, ContentType.Parse(file.ContentType));
            }
        }
        catch (DownloadExtractByFileNotFoundException)
        {
            return NotFound();
        }
    }
}

public record DownloadExtractByFileRequestBody(int Buffer, string Description, IFormFileCollection Files, bool IsInformative);
