namespace RoadRegistry.BackOffice.Api.Downloads;

using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Downloads;
using Be.Vlaanderen.Basisregisters.Api;
using Exceptions;
using Microsoft.AspNetCore.Mvc;
using FileCallbackResult = Framework.FileCallbackResult;

[ApiVersion("2.0")]
[AdvertiseApiVersions("2.0")]
[ApiRoute("download")]
[ApiExplorerSettings(GroupName = "Downloads")]
public class DownloadController : ControllerBase
{
    private readonly IMediator _mediator;

    public DownloadController(IMediator mediator) => _mediator = mediator;

    [HttpGet("for-editor")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadEditorRequest();
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
        }
        catch (DownloadEditorNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("for-product/{date}")]
    public async Task<IActionResult> Get(string date, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadProductRequest(date);
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
        }
        catch (DownloadProductNotFoundException)
        {
            return NotFound();
        }
    }
}
