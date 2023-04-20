namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Abstractions.RoadNetworks;
using BackOffice.Framework;
using FeatureToggles;
using FluentValidation;
using Messages;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    [HttpPost("snapshots/refresh")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostSnapshotRebuild([FromBody] RebuildSnapshotParameters parameters,
        [FromServices] RebuildSnapshotParametersValidator validator,
        [FromServices] UseSnapshotSqsRequestFeatureToggle snapshotFeatureToggle,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(parameters, HttpContext.RequestAborted);

        if (snapshotFeatureToggle.FeatureEnabled)
        {
            var response = await Mediator.Send(new RebuildRoadNetworkSnapshotRequest(), cancellationToken);
            return Ok(response);
        }

        var command = new RebuildRoadNetworkSnapshot();
        await RoadNetworkCommandQueue
            .Write(new Command(command), HttpContext.RequestAborted);
        return Accepted();
    }
}
