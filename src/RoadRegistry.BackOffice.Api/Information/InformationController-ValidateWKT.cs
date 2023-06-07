namespace RoadRegistry.BackOffice.Api.Information;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Information;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class InformationController
{
    /// <summary>
    ///     Validates the WKT.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost("validate-wkt", Name = nameof(ValidateWKT))]
    [SwaggerOperation(OperationId = nameof(ValidateWKT), Description = "")]
    public async Task<ValidateWktContourResponse> ValidateWKT([FromBody] ValidateWktContourRequestBody model, CancellationToken cancellationToken)
    {
        var request = new ValidateWktContourRequest(model.Contour ?? "");
        var response = await _mediator.Send(request, cancellationToken);

        return response;
    }
}

public class ValidateWktContourRequestBody
{
    public string Contour { get; set; }
}