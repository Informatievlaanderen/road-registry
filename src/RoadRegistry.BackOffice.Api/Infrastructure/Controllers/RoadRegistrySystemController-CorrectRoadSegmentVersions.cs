namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Abstractions.RoadSegments;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    private const string CorrectRoadSegmentVersionsRoute = "correct/roadsegmentversions";

    /// <summary>
    /// Corrects the road segment versions.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>IActionResult.</returns>
    [HttpPost(CorrectRoadSegmentVersionsRoute, Name = nameof(CorrectRoadSegmentVersions))]
    [SwaggerOperation(OperationId = nameof(CorrectRoadSegmentVersions), Description = "")]
    public async Task<IActionResult> CorrectRoadSegmentVersions(CancellationToken cancellationToken)
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
