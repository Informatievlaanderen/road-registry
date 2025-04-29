namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Abstractions.RoadSegments;
using BackOffice.Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

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
        var result = await Mediator.Send(new CorrectRoadSegmentOrganizationNamesSqsRequest
        {
            ProvenanceData = CreateProvenanceData(Modification.Update),
            Request = new CorrectRoadSegmentOrganizationNamesRequest()
        }, cancellationToken);
        return Accepted(result);
    }
}
