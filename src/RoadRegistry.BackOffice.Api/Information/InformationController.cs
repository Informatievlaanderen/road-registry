namespace RoadRegistry.BackOffice.Api.Information;

using Abstractions.Information;
using Be.Vlaanderen.Basisregisters.Api;
using Editor.Schema;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("information")]
[ApiExplorerSettings(GroupName = "Information")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
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

    [HttpPost("validate-wkt")]
    public async Task<IActionResult> ValidateWKT([FromBody] ValidateWktContourRequestBody model, CancellationToken cancellationToken)
    {
        var request = new ValidateWktContourRequest(model.Contour ?? "");
        var response = await _mediator.Send(request, cancellationToken);

        return Ok(response);
    }
}
