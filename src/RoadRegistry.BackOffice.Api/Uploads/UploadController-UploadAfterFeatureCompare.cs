namespace RoadRegistry.BackOffice.Api.Uploads;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class UploadController
{
    /// <summary>
    ///     Posts the upload after feature compare.
    /// </summary>
    /// <param name="archive">The archive.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Als het bestand goed ontvangen werd.</response>
    /// <response code="404">Als het extract niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(Name = nameof(UploadAfterFeatureCompare))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(ExtractDownloadNotFoundException))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(UploadAfterFeatureCompare), Description = "")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> UploadAfterFeatureCompare(IFormFile archive, CancellationToken cancellationToken)
    {
        return await PostUpload(archive, async () =>
        {
            UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            var request = new UploadExtractRequest(requestArchive);
            await _mediator.Send(request, cancellationToken);
            return Ok();
        });
    }
}
