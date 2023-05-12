namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractController
{
    private const string PostDownloadRequestByContourRoute = "downloadrequests/bycontour";

    /// <summary>
    ///     Requests the download by contour.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [HttpPost(PostDownloadRequestByContourRoute, Name = nameof(RequestDownloadByContour))]
    [SwaggerOperation(OperationId = nameof(RequestDownloadByContour), Description = "")]
    public async Task<ActionResult> RequestDownloadByContour([FromBody] DownloadExtractByContourRequestBody body, CancellationToken cancellationToken)
    {
        var request = new DownloadExtractByContourRequest(body.Contour, body.Buffer, body.Description);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody(response.DownloadId.ToString()));
    }
}

/// <summary>
///     Class DownloadExtractByContourRequestBody.
///     Implements the
///     <see cref="System.IEquatable{RoadRegistry.BackOffice.Api.Extracts.DownloadExtractByContourRequestBody}" />
/// </summary>
/// <seealso cref="System.IEquatable{RoadRegistry.BackOffice.Api.Extracts.DownloadExtractByContourRequestBody}" />
public record DownloadExtractByContourRequestBody
{
    /// <summary>
    ///     Class DownloadExtractByContourRequestBody.
    ///     Implements the
    ///     <see cref="System.IEquatable{RoadRegistry.BackOffice.Api.Extracts.DownloadExtractByContourRequestBody}" />
    /// </summary>
    /// <seealso cref="System.IEquatable{RoadRegistry.BackOffice.Api.Extracts.DownloadExtractByContourRequestBody}" />
    public DownloadExtractByContourRequestBody(int Buffer, string Contour, string Description)
    {
        this.Buffer = Buffer;
        this.Contour = Contour;
        this.Description = Description;
    }

    /// <summary>
    ///     Gets the buffer.
    /// </summary>
    /// <value>The buffer.</value>
    public int Buffer { get; init; }

    public string Contour { get; init; }
    public string Description { get; init; }

    public void Deconstruct(out int Buffer, out string Contour, out string Description)
    {
        Buffer = this.Buffer;
        Contour = this.Contour;
        Description = this.Description;
    }
}