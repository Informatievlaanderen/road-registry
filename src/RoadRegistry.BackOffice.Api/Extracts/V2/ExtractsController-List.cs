namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts.V2;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    [ProducesResponseType(typeof(ExtractListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(GetList))]
    [HttpGet("", Name = nameof(GetList))]
    public async Task<ActionResult> GetList(
        CancellationToken cancellationToken)
    {
        var organizationCode = ApiContext.HttpContextAccessor.HttpContext.GetOperatorName();
        if (organizationCode is null)
        {
            throw new InvalidOperationException("User is authenticated but no operator could be found.");
        }

        var request = new ExtractListRequest(organizationCode);
        var response = await _mediator.Send(request, cancellationToken);

        return Ok(response);
    }
}
