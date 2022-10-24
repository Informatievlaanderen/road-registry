namespace RoadRegistry.BackOffice.Api.Information;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Api;
using Editor.Schema;
using FluentValidation;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("information")]
[ApiExplorerSettings(GroupName = "Information")]
[ApiKeyAuth("Road")]
public class InformationController : ControllerBase
{
    private readonly IMediator _mediator;

    public InformationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> Get([FromServices] EditorContext context)
    {
        var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(HttpContext.RequestAborted);
        if (info == null || !info.CompletedImport) return StatusCode(StatusCodes.Status503ServiceUnavailable);
        return new JsonResult(RoadNetworkInformationResponse.From(info));
    }

    [HttpPost("wkt-validation")]
    public async Task<IActionResult> ValidateWKT([FromBody] string wktContour, CancellationToken cancellationToken)
    {
        var request = new ValidateWktContourRequest();
        var response = await _mediator.Send(request, cancellationToken);
        return new ValidateWktContourResponse(response);
    }
}
