namespace RoadRegistry.BackOffice.Api.Downloads;

using Contracts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api;
using Contracts.Downloads;
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
    public async Task<FileCallbackResult> Get(CancellationToken cancellationToken)
    {
        var request = new DownloadEditorRequest();
        var response = await _mediator.Send(request, cancellationToken);
        return new(response);
    }

    [HttpGet("for-product/{date}")]
    public async Task<FileCallbackResult> Get(string date, CancellationToken cancellationToken)
    {
        var request = new DownloadProductRequest(date);
        var response = await _mediator.Send(request, cancellationToken);
        return new(response);
    }
}
