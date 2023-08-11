namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadNodes;
using Handlers.Sqs.RoadNodes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadRegistrySystemController
{
    private const string CorrectRoadNodeVersionsRoute = "correct/roadnodeversions";

    /// <summary>
    ///     Corrects the road node versions.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(CorrectRoadNodeVersionsRoute, Name = nameof(CorrectRoadNodeVersions))]
    [SwaggerOperation(OperationId = nameof(CorrectRoadNodeVersions), Description = "")]
    public async Task<IActionResult> CorrectRoadNodeVersions(CancellationToken cancellationToken)
    {
        await Mediator.Send(Enrich(new CorrectRoadNodeVersionsSqsRequest
        {
            Request = new CorrectRoadNodeVersionsRequest()
        }), cancellationToken);
        return Accepted();
    }
}
