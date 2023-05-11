namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using FeatureToggles;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    private const string DeleteOrganizationRoute = "organization/{id}";

    /// <summary>
    /// Deletes the organization.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="featureToggle">The feature toggle.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>IActionResult.</returns>
    [HttpDelete(DeleteOrganizationRoute,  Name = nameof(DeleteOrganization))]
    [SwaggerOperation(OperationId = nameof(DeleteOrganization), Description = "")]
    public async Task<IActionResult> DeleteOrganization([FromRoute] string id,
        [FromServices] UseOrganizationDeleteFeatureToggle featureToggle,
        [FromServices] IValidator<DeleteOrganization> validator,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        var command = new DeleteOrganization
        {
            Code = id
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await RoadNetworkCommandQueue
            .Write(new Command(command), HttpContext.RequestAborted);

        return Accepted();
    }
}
