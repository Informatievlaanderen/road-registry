namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using System;
using System.Collections.Generic;
using Abstractions.RoadNetworks;
using BackOffice.Framework;
using FeatureToggles;
using FluentValidation;
using Messages;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Snapshot.Handlers.Sqs.RoadNetworks;

public partial class RoadRegistrySystemController
{
    [HttpPost("snapshots/refresh")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostSnapshotRebuild([FromBody] RebuildSnapshotParameters parameters,
        [FromServices] RebuildSnapshotParametersValidator validator,
        [FromServices] UseSnapshotSqsRequestFeatureToggle snapshotFeatureToggle,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(parameters, cancellationToken);

        if (snapshotFeatureToggle.FeatureEnabled)
        {
            await Mediator.Send(new RebuildRoadNetworkSnapshotSqsRequest
            {
                ProvenanceData = new RoadRegistryProvenanceData(),
                Metadata = new Dictionary<string, object>
                {
                    { "CorrelationId", Guid.NewGuid() }
                },
                Request = new RebuildRoadNetworkSnapshotRequest()
            }, cancellationToken);
            return Accepted();
        }

        var command = new RebuildRoadNetworkSnapshot();
        await RoadNetworkCommandQueue
            .Write(new Command(command), cancellationToken);
        return Accepted();
    }
}
