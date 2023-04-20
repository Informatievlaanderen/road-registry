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
    [HttpPost("organization")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostOrganizationCreate([FromBody] OrganizationCreateParameters parameters,
        [FromServices] UseOrganizationCreateFeatureToggle featureToggle,
        [FromServices] IValidator<CreateOrganization> validator,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        var command = new CreateOrganization
        {
            Code = parameters?.Code,
            Name = parameters?.Name
        };
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        await RoadNetworkCommandQueue
            .Write(new Command(command), HttpContext.RequestAborted);

        return Accepted();
    }
}

public sealed record OrganizationCreateParameters(string Code, string Name);
