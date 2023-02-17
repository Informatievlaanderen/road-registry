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
    public async Task<IActionResult> PostSnapshotRebuild([FromBody] RebuildSnapshotParameters parameters,
        [FromServices] UseSnapshotRebuildFeatureToggle featureToggle,
        [FromServices] RebuildSnapshotParametersValidator validator)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        await validator.ValidateAndThrowAsync(parameters, HttpContext.RequestAborted);

        var command = new RebuildRoadNetworkSnapshot
        {
        };
        await CommandQueue
            .Write(new Command(command), HttpContext.RequestAborted);

        return Ok();
    }
}
