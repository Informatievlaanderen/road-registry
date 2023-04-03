namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Abstractions.RoadNetworks;
using FeatureToggles;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Snapshot.Handlers.Sqs.RoadNetworks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Framework;
using Messages;

public partial class RoadRegistrySystemController
{
    [HttpPost("snapshots/refresh")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostSnapshotRebuild([FromBody] RebuildSnapshotParameters parameters,
        [FromServices] UseSnapshotRebuildFeatureToggle featureToggle,
        [FromServices] RebuildSnapshotParametersValidator validator,
        [FromServices] UseSnapshotSqsRequestFeatureToggle snapshotFeatureToggle,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        await validator.ValidateAndThrowAsync(parameters, HttpContext.RequestAborted);

        if (snapshotFeatureToggle.FeatureEnabled)
        {
            await Mediator.Send(new RebuildRoadNetworkSnapshotSqsRequest
            {
                ProvenanceData = new RoadRegistryProvenanceData(),
                Metadata = new Dictionary<string, object?>
                {
                    { "CorrelationId", Guid.NewGuid() }
                },
                Request = new RebuildRoadNetworkSnapshotRequest()
            }, cancellationToken);
        }
        else
        {
            var command = new RebuildRoadNetworkSnapshot();
            await RoadNetworkCommandQueue
                .Write(new Command(command), HttpContext.RequestAborted);
        }

        return Ok();
    }
}
