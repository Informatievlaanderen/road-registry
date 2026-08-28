namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Marten;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;

public partial class RoadSegmentsController
{
    // What every status change endpoint does. They differ in nothing but the transition they name: none of them
    // carries a body, because what happens follows entirely from the network around the segment.
    private async Task<IActionResult> ChangeRoadSegmentStatusV2(
        RoadSegmentStatusChange statusChange,
        RoadSegmentIdValidator idValidator,
        int id,
        IDocumentStore store,
        CancellationToken cancellationToken)
    {
        try
        {
            // VAL-1
            await idValidator.ValidateRoadSegmentIdAndThrowAsync(id, cancellationToken);

            await using var session = store.LightweightSession();

            // VAL-2, VAL-3. Everything else - does the segment have the status it is changing away from, is there a
            // road node within reach, does it cross anything - is validated by the domain, which is the only place
            // that knows the surrounding network.
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(id, cancellationToken);
            if (roadSegment is null)
            {
                return NotFound();
            }

            if (roadSegment.IsRemoved)
            {
                return new StatusCodeResult(StatusCodes.Status410Gone);
            }

            var sqsRequest = new ChangeRoadSegmentStatusV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                RoadSegmentId = new RoadSegmentId(id),
                StatusChange = statusChange,
                // Only a holder of the 'ingemeten' scope may change the status of a measured road segment. The
                // entitlement travels with the request; the domain decides whether it is needed.
                MayModifyMeasuredRoadSegments = HasIngemetenWegScope()
            };
            var result = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}
