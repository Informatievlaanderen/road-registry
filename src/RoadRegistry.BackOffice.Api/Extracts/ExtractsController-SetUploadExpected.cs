namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    private const string SetUploadExpectedRoute = "{downloadId}/expected";

    [HttpPut(SetUploadExpectedRoute, Name = nameof(SetUploadExpected))]
    [SwaggerOperation(OperationId = nameof(SetUploadExpected), Description = "")]
    public async Task<IActionResult> SetUploadExpected(
        [FromRoute] string downloadId,
        [FromBody] ExtractUploadExpectedRequestBody requestBody,
        [FromServices] ExtractUploadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!DownloadId.CanParse(downloadId)) throw new DownloadExtractNotFoundException("");

            var request = new ExtractUploadExpectedRequest(DownloadId.Parse(downloadId), requestBody.UploadExpected);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted();
        }
        catch (UploadExtractNotFoundException exception)
        {
            return NotFound();
        }
    }
}

public sealed record ExtractUploadExpectedRequestBody
{
    public bool UploadExpected { get; set; }
}
