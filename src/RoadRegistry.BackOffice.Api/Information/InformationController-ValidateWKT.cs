namespace RoadRegistry.BackOffice.Api.Information;

using Abstractions.Information;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

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
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.IngemetenWeg.Beheerder)]
    [SwaggerOperation(OperationId = nameof(ValidateWKT), Description = "")]
    public async Task<ValidateWktContourResponse> ValidateWKT([FromBody] ValidateWktContourRequestBody model, CancellationToken cancellationToken)
    {
        try
        {
            var request = new ValidateWktContourRequest(model.Contour ?? string.Empty);
            var response = await _mediator.Send(request, cancellationToken);

            return response;
        }
        catch
        {
            return new ValidateWktContourResponse { IsValid = false };
        }
    }
}

public class ValidateWktContourRequestBody
{
    public string Contour { get; set; }
}
