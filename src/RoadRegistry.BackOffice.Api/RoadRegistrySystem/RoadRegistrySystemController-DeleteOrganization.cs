namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using System.Threading;
using System.Threading.Tasks;
using BackOffice.Framework;
using FeatureToggles;
using FluentValidation;
using Messages;
using Microsoft.AspNetCore.Mvc;

public partial class RoadRegistrySystemController
{
    [HttpDelete("organization/{id}")]
    [ApiExplorerSettings(IgnoreApi = true)]
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
