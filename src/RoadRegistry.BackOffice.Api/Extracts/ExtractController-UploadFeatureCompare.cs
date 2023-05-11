namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UploadExtractFeatureCompareRequest = Abstractions.Extracts.UploadExtractFeatureCompareRequest;

public partial class ExtractController
{
    private const string PostFeatureCompareUploadRoute = "download/{downloadId}/uploads/fc";

    /// <summary>
    /// Uploads the feature compare.
    /// </summary>
    /// <param name="downloadId">The download identifier.</param>
    /// <param name="archive">The archive.</param>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;IActionResult&gt;.</returns>
    [HttpPost(PostFeatureCompareUploadRoute, Name = nameof(UploadFeatureCompare))]
    [SwaggerOperation(OperationId = nameof(UploadFeatureCompare), Description = "")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public Task<IActionResult> UploadFeatureCompare(
        [FromRoute] string downloadId,
        IFormFile archive,
        CancellationToken cancellationToken)
    {
        return PostUpload(archive, async () =>
        {
            var requestArchive = new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            var request = new UploadExtractFeatureCompareRequest(downloadId, requestArchive);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new UploadExtractFeatureCompareResponseBody(response.ArchiveId));
        });
    }
}

public record UploadExtractFeatureCompareResponseBody(string ArchiveId);
