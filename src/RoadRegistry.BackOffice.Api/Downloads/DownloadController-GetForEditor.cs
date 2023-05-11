namespace RoadRegistry.BackOffice.Api.Downloads;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Downloads;
using Abstractions.Exceptions;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class DownloadController
{
    private const string GetForEditorRoute = "for-editor";

    /// <summary>
    /// Gets for editor.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>IActionResult.</returns>
    [HttpGet(GetForEditorRoute, Name = nameof(GetForEditor))]
    [SwaggerOperation(OperationId = nameof(GetForEditor), Description = "")]
    public async Task<IActionResult> GetForEditor(CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadEditorRequest();
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
        }
        catch (DownloadEditorNotFoundException ex)
        {
            return new StatusCodeResult((int)ex.HttpStatusCode);
        }
    }
}
