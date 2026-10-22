namespace RoadRegistry.BackOffice.Api.V2.RoadNodes;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentValidation;
using FluentValidation.Results;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadNodesController
{
    private const string RemoveRoadNodeRoute = "{id}/acties/verwijderen";

    /// <summary>
    ///     Verwijder een wegknoop, waarbij de aansluitende wegsegmenten samengevoegd worden.
    /// </summary>
    /// <param name="id">De identificator van de wegknoop.</param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als de wegknoop niet gevonden kan worden.</response>
    /// <response code="410">Als de wegknoop is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(RemoveRoadNodeRoute, Name = nameof(RemoveRoadNodeV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadNodeNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(RoadNodeGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(RemoveRoadNodeV2), Description = "Verwijder een wegknoop. De aansluitende wegsegmenten worden hierbij samengevoegd: twee bij een validatieknoop, paarsgewijs bij een echte knoop met precies vier aansluitende wegsegmenten.")]
    public async Task<IActionResult> RemoveRoadNodeV2(
        [FromRoute] int id,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-1
            if (!RoadNodeId.Accepts(id))
            {
                throw new ValidationException([new ValidationFailure("id", $"De waarde {id} is ongeldig.")]);
            }

            await using var session = store.LightweightSession();

            // VAL-2, VAL-3. Whether this node is one that can be undone at all, and whether the roads it holds can
            // become one, is the domain's to say: it is the only place that knows what hangs off it.
            var roadNode = await session.LoadAsync<RoadNodeReadItem>(id, cancellationToken);
            if (roadNode is null)
            {
                return NotFound();
            }

            if (roadNode.IsRemoved)
            {
                return new StatusCodeResult(StatusCodes.Status410Gone);
            }

            var sqsRequest = new RemoveRoadNodeV2SqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Delete),
                RoadNodeId = new RoadNodeId(id)
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
