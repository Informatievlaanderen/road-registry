namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadRegistrySystemController
{
    private const string CorrectRoadSegmentVersionsRoute = "correct/roadsegmentversions";

    /// <summary>
    ///     Corrects the road segment versions.
    /// </summary>
    /// <param name="parameters">
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(CorrectRoadSegmentVersionsRoute, Name = nameof(CorrectRoadSegmentVersions))]
    [SwaggerOperation(OperationId = nameof(CorrectRoadSegmentVersions), Description = "")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<IActionResult> CorrectRoadSegmentVersions([FromBody] CorrectRoadSegmentVersionsParameters parameters, CancellationToken cancellationToken)
    {
        await Mediator.Send(new CorrectRoadSegmentVersionsSqsRequest
        {
            Request = new CorrectRoadSegmentVersionsRequest(parameters?.RoadSegmentIds)
        }, cancellationToken);
        return Accepted();
    }

    public class CorrectRoadSegmentVersionsParameters
    {
        public List<int> RoadSegmentIds { get; set; }
    }
}
