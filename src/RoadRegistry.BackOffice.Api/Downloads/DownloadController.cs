namespace RoadRegistry.BackOffice.Api.Downloads;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Downloads;
using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Api;
using Framework;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("download")]
[ApiExplorerSettings(GroupName = "Downloads")]
[ApiKeyAuth("Road")]
public class DownloadController : ControllerBase
{
    private readonly IMediator _mediator;

    public DownloadController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("for-editor")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
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
