namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Abstractions.RoadSegments;
using BackOffice.Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    private const string MigrateOutlinedRoadSegmentsOutOfRoadNetworkRoute = "migrate/outlinedroadsegmentsoutofroadnetwork";

    /// <summary>
    ///     Migrated outlined road segments in the roadnetwork stream to their own stream
    /// </summary>
    /// <param name="parameters" />
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(MigrateOutlinedRoadSegmentsOutOfRoadNetworkRoute, Name = nameof(MigrateOutlinedRoadSegmentsOutOfRoadNetwork))]
    [SwaggerOperation(OperationId = nameof(MigrateOutlinedRoadSegmentsOutOfRoadNetwork), Description = "")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<IActionResult> MigrateOutlinedRoadSegmentsOutOfRoadNetwork([FromBody] MigrateOutlinedRoadSegmentsOutOfRoadNetworkParameters parameters, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new MigrateOutlinedRoadSegmentsOutOfRoadNetworkSqsRequest
        {
            Request = new MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequest()
        }, cancellationToken);
        return Accepted(result);
    }

    public class MigrateOutlinedRoadSegmentsOutOfRoadNetworkParameters
    {
    }
}
