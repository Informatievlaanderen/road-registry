namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadRegistrySystemController
{
    private const string CorrectRoadSegmentOrganizationNamesRoute = "correct/roadsegmentorganizationnames";

    /// <summary>
    ///     Corrects the road segments with empty organization names.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(CorrectRoadSegmentOrganizationNamesRoute, Name = nameof(CorrectRoadSegmentOrganizationNames))]
    [SwaggerOperation(OperationId = nameof(CorrectRoadSegmentOrganizationNames), Description = "")]
    public async Task<IActionResult> CorrectRoadSegmentOrganizationNames(CancellationToken cancellationToken)
    {
        await Mediator.Send(new CorrectRoadSegmentOrganizationNamesSqsRequest
        {
            Request = new CorrectRoadSegmentOrganizationNamesRequest()
        }, cancellationToken);
        return Accepted();
    }
}
