namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Abstractions.RoadSegments;
using BackOffice.Handlers.Sqs.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    private const string CorrectRoadSegmentVersionsRoute = "correct/roadsegmentversions";

    /// <summary>
    ///     Corrects the road segment versions.
    /// </summary>
    /// <param name="parameters" />
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
        if (parameters?.RoadSegments?.Any() == true)
        {
            var uniqueIdsCount = parameters.RoadSegments.Select(x => x.Id).Distinct().Count();
            if (uniqueIdsCount != parameters.RoadSegments.Count)
            {
                throw new ApiException("Roadsegment IDs must be unique.", 400);
            }
        }

        await Mediator.Send(new CorrectRoadSegmentVersionsSqsRequest
        {
            Request = new CorrectRoadSegmentVersionsRequest(parameters
                ?.RoadSegments
                ?.Select(x => new CorrectRoadSegmentVersion(x.Id, x.Version, x.GeometryVersion))
                .ToList())
        }, cancellationToken);
        return Accepted();
    }

    public class CorrectRoadSegmentVersionsParameters
    {
        public List<RoadSegmentVersion> RoadSegments { get; set; }
    }

    public class RoadSegmentVersion
    {
        public int Id { get; set; }
        public int? Version { get; set; }
        public int? GeometryVersion { get; set; }
    }
}
