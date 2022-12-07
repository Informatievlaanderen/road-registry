namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using System.Threading.Tasks;
using BackOffice.Framework;
using FeatureToggles;
using FluentValidation;
using Messages;
using Microsoft.AspNetCore.Mvc;

public partial class RoadRegistrySystemController
{
    [HttpPost("snapshots/refresh")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostSnapshotRebuild([FromBody] RequestSnapshotRebuildParameters request,
        [FromServices] IValidator<RebuildSnapshotParameters> validator,
        [FromServices] UseSnapshotRebuildFeatureToggle featureToggle)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        var parameters = new RebuildSnapshotParameters { StartFromVersion = request?.StartFromVersion ?? 0 };
        await validator.ValidateAndThrowAsync(parameters, HttpContext.RequestAborted);

        var command = new RebuildRoadNetworkSnapshot
        {
            StartFromVersion = parameters.StartFromVersion
        };
        await new RoadNetworkCommandQueue(Store)
            .Write(new Command(command), HttpContext.RequestAborted);

        return Ok();
    }
}

public sealed record RequestSnapshotRebuildParameters(int StartFromVersion);
