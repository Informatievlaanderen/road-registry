namespace RoadRegistry.BackOffice.Api.RoadRegistrySystem;

using Abstractions.RoadSegments;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    [HttpPost("correct/roadsegmentversions")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostCorrectRoadSegmentVersions(CancellationToken cancellationToken)
    {
        await Mediator.Send(new CorrectRoadSegmentVersionsSqsRequest
        {
            ProvenanceData = new RoadRegistryProvenanceData(),
            Metadata = new Dictionary<string, object>
            {
                { "CorrelationId", Guid.NewGuid() }
            },
            Request = new CorrectRoadSegmentVersionsRequest()
        }, cancellationToken);
        return Accepted();
    }
}
