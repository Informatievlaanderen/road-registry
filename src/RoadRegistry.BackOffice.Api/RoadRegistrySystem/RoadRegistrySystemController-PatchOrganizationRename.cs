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
    [HttpPatch("organization/{id}/rename")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PatchOrganizationRename([FromBody] OrganizationRenameParameters parameters,
        [FromRoute] string id,
        [FromServices] UseOrganizationRenameFeatureToggle featureToggle,
        [FromServices] IValidator<RenameOrganization> validator,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        var command = new RenameOrganization
        {
            Code = id,
            Name = parameters?.Name
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await new RoadNetworkCommandQueue(Store)
            .Write(new Command(command), HttpContext.RequestAborted);

        return Ok();
    }
}

public sealed record OrganizationRenameParameters(string Name);
