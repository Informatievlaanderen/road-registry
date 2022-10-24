namespace RoadRegistry.BackOffice.Api.Information;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Information;
using Be.Vlaanderen.Basisregisters.Api;
using Editor.Schema;
using FluentValidation;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Version = Infrastructure.Version;

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

    [HttpPost("validate-wkt")]
    public async Task<IActionResult> ValidateWKT(ValidateWktContourRequestBody model, CancellationToken cancellationToken)
    {
        var request = new ValidateWktContourRequest(model.Contour ?? "");
        var response = await _mediator.Send(request, cancellationToken);
        return response.Exception is null
            ? Ok()
            : UnprocessableEntity();
    }
}
