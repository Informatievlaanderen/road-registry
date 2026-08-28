namespace RoadRegistry.BackOffice.Api.V2.RoadSegments;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using RoadRegistry.BackOffice.Api.Infrastructure.Authentication;
using RoadRegistry.RoadSegment.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string CorrectRealizedToPlannedRoute = "{id}/acties/corrigeren/gerealiseerdnaargepland";

    /// <summary>
    ///     Corrigeer een gerealiseerd wegsegment naar gepland.
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
    [HttpPost(CorrectRealizedToPlannedRoute, Name = nameof(CorrectRoadSegmentFromRealizedToPlannedV2))]
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
    [SwaggerOperation(OperationId = nameof(CorrectRoadSegmentFromRealizedToPlannedV2), Description = "Corrigeer een gerealiseerd wegsegment naar gepland. Het wegsegment wordt losgemaakt van het wegennet: begin- en eindknoop worden verwijderd waar ze niets meer dragen, de kruisingen waartoe het wegsegment behoort verdwijnen, en de wegknooptypes van aansluitende wegsegmenten worden aangepast waar nodig.")]
    public Task<IActionResult> CorrectRoadSegmentFromRealizedToPlannedV2(
        [FromServices] RoadSegmentIdValidator idValidator,
        [FromRoute] int id,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        return ChangeRoadSegmentStatusV2(RoadSegmentStatusChange.RealizedToPlanned, idValidator, id, store, cancellationToken);
    }
}
