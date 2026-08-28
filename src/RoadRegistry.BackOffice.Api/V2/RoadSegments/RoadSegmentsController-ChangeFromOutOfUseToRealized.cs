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
    private const string ChangeFromOutOfUseToRealizedRoute = "{id}/acties/buitengebruiknaargerealiseerd";

    /// <summary>
    ///     Markeer een buiten gebruik zijnd wegsegment als gerealiseerd.
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
    [HttpPost(ChangeFromOutOfUseToRealizedRoute, Name = nameof(ChangeRoadSegmentFromOutOfUseToRealizedV2))]
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
    [SwaggerOperation(OperationId = nameof(ChangeRoadSegmentFromOutOfUseToRealizedV2), Description = "Markeer een buiten gebruik zijnd wegsegment als gerealiseerd. Het wegsegment wordt aan het wegennet geknoopt: de uiteinden worden naar bestaande wegknopen binnen 1 meter gesnapt, waar er geen ligt komt een eindknoop, en kruisingen met gerealiseerde wegsegmenten worden als gelijkgrondse kruising vastgelegd.")]
    public Task<IActionResult> ChangeRoadSegmentFromOutOfUseToRealizedV2(
        [FromServices] RoadSegmentIdValidator idValidator,
        [FromRoute] int id,
        [FromServices] IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        return ChangeRoadSegmentStatusV2(RoadSegmentStatusChange.OutOfUseToRealized, idValidator, id, store, cancellationToken);
    }
}
