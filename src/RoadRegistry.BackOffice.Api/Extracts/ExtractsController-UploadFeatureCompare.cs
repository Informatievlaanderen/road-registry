namespace RoadRegistry.BackOffice.Api.Extracts;

using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using Uploads;
using UploadExtractRequest = Abstractions.Extracts.UploadExtractRequest;

public partial class ExtractsController
{
    private const string PostFeatureCompareUploadRoute = "download/{downloadId}/uploads/fc";

    /// <summary>
    ///     Upload before feature compare archive.
    /// </summary>
    /// <param name="downloadId"></param>
    /// <param name="archive">The archive.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
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
            var request = new UploadExtractRequest(downloadId, requestArchive, null);

            var response = await _mediator.Send(request, cancellationToken);

            return Accepted(new UploadExtractFeatureCompareResponseBody(response.UploadId.ToString(), ChangeRequestId.FromUploadId(response.UploadId)));
        });
    }
}
