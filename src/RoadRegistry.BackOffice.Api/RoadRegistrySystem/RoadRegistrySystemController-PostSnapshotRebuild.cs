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

public partial class RoadRegistrySystemController
{
    [HttpPost("snapshots/refresh")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostSnapshotRebuild([FromBody] RebuildSnapshotParameters parameters,
        [FromServices] UseSnapshotRebuildFeatureToggle featureToggle,
        [FromServices] RebuildSnapshotParametersValidator validator,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        await validator.ValidateAndThrowAsync(parameters, HttpContext.RequestAborted);

        await Mediator.Send(new CreateRoadNetworkSnapshotSqsRequest
        {
            ProvenanceData = new RoadRegistryProvenanceData(),
            Metadata = new Dictionary<string, object?>
            {
                { "CorrelationId", Guid.NewGuid() }
            },
            Request = new CreateRoadNetworkSnapshotRequest { StreamVersion = parameters.StartFromVersion }
        }, cancellationToken);

        //var command = new RebuildRoadNetworkSnapshot();
        //await CommandQueue
        //    .Write(new Command(command), HttpContext.RequestAborted);

        return Ok();
    }
}
