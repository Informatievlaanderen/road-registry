namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UploadExtractRequest = Abstractions.Extracts.UploadExtractRequest;

public partial class ExtractsController
{
    private const string PostUploadRoute = "download/{downloadId}/uploads";

    /// <summary>
    ///     Upload after feature compare archive.
    /// </summary>
    /// <param name="downloadId">The download identifier.</param>
    /// <param name="archive">The archive.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>Task&lt;IActionResult&gt;.</returns>
    [HttpPost(PostUploadRoute, Name = nameof(Upload))]
    [SwaggerOperation(OperationId = nameof(Upload), Description = "")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public Task<IActionResult> Upload(
        [FromRoute] string downloadId,
        IFormFile archive,
        CancellationToken cancellationToken)
    {
        return PostUpload(archive, async () =>
        {
            var response = await _mediator.Send(
                new UploadExtractRequest(
                    downloadId,
                    new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType))
                ), cancellationToken);
            return Accepted(new UploadExtractResponseBody(response.UploadId.ToString()));
        });
    }
}

public record UploadExtractResponseBody(string UploadId);
