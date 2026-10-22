namespace RoadRegistry.BackOffice.Api.V2.GradeSeparatedJunctions;

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
using RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class GradeSeparatedJunctionsController
{
    private const string ChangeToGradeJunctionRoute = "{id}/acties/wijzigen/naargelijkgrondsekruising";

    /// <summary>
    ///     Wijzig een ongelijkgrondse kruising naar een gelijkgrondse kruising.
    /// </summary>
    /// <param name="id">De identificator van de ongelijkgrondse kruising.</param>
    /// <param name="store"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het verzoek aanvaard is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als de ongelijkgrondse kruising niet gevonden kan worden.</response>
    /// <response code="410">Als de ongelijkgrondse kruising is verwijderd.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeToGradeJunctionRoute, Name = nameof(ChangeGradeSeparatedJunctionToGradeJunctionV2))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", JsonSchemaType.String, "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", JsonSchemaType.String, "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GradeSeparatedJunctionNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(GradeSeparatedJunctionGoneResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(ChangeGradeSeparatedJunctionToGradeJunctionV2), Description = "Wijzig een ongelijkgrondse kruising naar een gelijkgrondse kruising.")]
    public async Task<IActionResult> ChangeGradeSeparatedJunctionToGradeJunctionV2(
        [FromRoute] int id,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // VAL-1
            if (!GradeSeparatedJunctionId.Accepts(id))
            {
                throw new ValidationException([new ValidationFailure("id", $"De waarde {id} is ongeldig.")]);
            }

            await using var session = store.LightweightSession();

            // VAL-2, VAL-3. There is nothing else to validate: which road becomes 'wegsegment 1' and which
            // 'wegsegment 2' does not matter, so the request carries no body at all.
            var gradeSeparatedJunction = await session.LoadAsync<GradeSeparatedJunctionReadItem>(id, cancellationToken);
            if (gradeSeparatedJunction is null)
            {
                return NotFound();
            }

            if (gradeSeparatedJunction.IsRemoved)
            {
                return new StatusCodeResult(StatusCodes.Status410Gone);
            }

            var sqsRequest = new ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Update),
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(id)
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
