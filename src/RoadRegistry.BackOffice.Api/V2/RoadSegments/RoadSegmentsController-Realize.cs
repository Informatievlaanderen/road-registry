namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using RoadRegistry.BackOffice.Abstractions.Extensions;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string RealizeRoute = "{id}/acties/geplandnaargerealiseerd";

    /// <summary>
    ///     Markeer een gepland wegsegment als gerealiseerd.
    /// </summary>
    /// <param name="idValidator"></param>
    /// <param name="id"></param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="410">Als het wegsegment is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(RealizeRoute, Name = nameof(RealizeRoadSegmentV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(RoadSegmentGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(RealizeRoadSegmentV2), Description = "Markeer een gepland wegsegment als gerealiseerd. Het wegsegment wordt aan het wegennet geknoopt: de uiteinden worden naar bestaande wegknopen binnen 1 meter gesnapt, waar er geen ligt komt een eindknoop, en kruisingen met gerealiseerde wegsegmenten worden als gelijkgrondse kruising vastgelegd.")]
    public async Task<IActionResult> RealizeRoadSegmentV2(
        [FromServices] RoadSegmentIdValidator idValidator,
        [FromRoute] int id,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-1
            await idValidator.ValidateRoadSegmentIdAndThrowAsync(id, cancellationToken);

            await using var session = store.LightweightSession();

            // VAL-2, VAL-3. Everything else - does it have status 'gepland', is there a road node within reach, does
            // it cross anything - is validated by the domain, which is the only place that knows the surrounding
            // network. The request carries no body: what happens follows entirely from that network.
            var roadSegment = await session.LoadAsync<RoadSegmentReadItem>(id, cancellationToken);
            if (roadSegment is null)
            {
                return NotFound();
            }

            if (roadSegment.IsRemoved)
            {
                return new StatusCodeResult(StatusCodes.Status410Gone);
            }

            var sqsRequest = new RealizeRoadSegmentV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                RoadSegmentId = new RoadSegmentId(id),
                // VAL-9: only a holder of the 'ingemeten' scope may realize a measured road segment. The entitlement
                // travels with the request; the domain decides whether it is needed.
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
