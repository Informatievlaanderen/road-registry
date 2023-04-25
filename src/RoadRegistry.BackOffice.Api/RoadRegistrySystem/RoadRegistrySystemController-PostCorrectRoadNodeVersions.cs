namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Abstractions.RoadNodes;
using Microsoft.AspNetCore.Mvc;
using Handlers.Sqs.RoadNodes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    [HttpPost("correct/roadnodeversions")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostCorrectRoadNodeVersions(CancellationToken cancellationToken)
    {
        await Mediator.Send(new CorrectRoadNodeVersionsSqsRequest
        {
            ProvenanceData = new RoadRegistryProvenanceData(),
            Metadata = new Dictionary<string, object>
            {
                { "CorrelationId", Guid.NewGuid() }
            },
            Request = new CorrectRoadNodeVersionsRequest()
        }, cancellationToken);
        return Accepted();
    }
}
