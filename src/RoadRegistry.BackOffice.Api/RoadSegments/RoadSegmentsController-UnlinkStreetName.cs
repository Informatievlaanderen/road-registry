namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extensions;
using Abstractions.RoadSegments;
using BackOffice.Extracts.Dbase.RoadSegments;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Editor.Projections;
using Editor.Schema;
using FeatureToggles;
using FluentValidation;
using Handlers.Sqs.RoadSegments;
using Infrastructure;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string UnlinkStreetNameRoute = "{id}/acties/straatnaamontkoppelen";

    /// <summary>
    ///     Ontkoppel een straatnaam van een wegsegment
    /// </summary>
    /// <param name="featureToggle"></param>
    /// <param name="ifMatchHeaderValidator"></param>
    /// <param name="parameters"></param>
    /// <param name="id">Identificator van het wegsegment.</param>
    /// <param name="ifMatchHeaderValue"></param>
    /// <param name="idValidator"></param>
    /// <param name="validator"></param>
    /// <param name="editorContext"></param>
    /// <param name="manager"></param>
    /// <param name="fileEncoding"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(UnlinkStreetNameRoute, Name = nameof(UnlinkStreetName))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenAttribuutWaarden.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", "string", "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", "string", "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(PostUnlinkStreetNameParameters), typeof(PostUnlinkStreetNameParametersExamples))]
    [SwaggerOperation(OperationId = nameof(UnlinkStreetName), Description = "Ontkoppel een linker- en/of rechterstraatnaam van een wegsegment waaraan momenteel een linker- en/of rechterstraatnaam gekoppeld is.")]
    public async Task<IActionResult> UnlinkStreetName(
        [FromServices] UseRoadSegmentUnlinkStreetNameFeatureToggle featureToggle,
        [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
        [FromServices] RoadSegmentIdValidator idValidator,
        [FromServices] IValidator<UnlinkStreetNameRequest> validator,
        [FromServices] EditorContext editorContext,
        [FromServices] RecyclableMemoryStreamManager manager,
        [FromServices] FileEncoding fileEncoding,
        [FromBody] PostUnlinkStreetNameParameters parameters,
        [FromRoute] int id,
        [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            await idValidator.ValidateRoadSegmentIdAndThrowAsync(id, cancellationToken);

            if (!await ifMatchHeaderValidator.IsValid(ifMatchHeaderValue, new RoadSegmentId(id), cancellationToken))
            {
                return new PreconditionFailedResult();
            }

            var roadSegment = await editorContext.RoadSegments.FindAsync(new object[] { id }, cancellationToken);
            var roadSegmentDbaseRecord = new RoadSegmentDbaseRecord().FromBytes(roadSegment!.DbaseRecord, manager, fileEncoding);
            var geometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord.METHODE.Value];

            var request = new UnlinkStreetNameRequest(id, geometryDrawMethod, parameters?.LinkerstraatnaamId, parameters?.RechterstraatnaamId);
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var result = await _mediator.Send(new UnlinkStreetNameSqsRequest { Request = request }, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

[DataContract(Name = "StraatnaamOntkoppelen", Namespace = "")]
public class PostUnlinkStreetNameParameters
{
    /// <summary>
    ///     De unieke en persistente identificator van de straatnaam aan de linkerzijde van het wegsegment.
    /// </summary>
    [DataMember(Name = "LinkerstraatnaamId", Order = 1)]
    [JsonProperty]
    public string LinkerstraatnaamId { get; set; }

    /// <summary>
    ///     De unieke en persistente identificator van de straatnaam aan de rechterzijde van het wegsegment.
    /// </summary>
    [DataMember(Name = "RechterstraatnaamId", Order = 2)]
    [JsonProperty]
    public string RechterstraatnaamId { get; set; }
}

public class PostUnlinkStreetNameParametersExamples : IExamplesProvider<PostUnlinkStreetNameParameters>
{
    public PostUnlinkStreetNameParameters GetExamples()
    {
        return new PostUnlinkStreetNameParameters
        {
            LinkerstraatnaamId = "https://data.vlaanderen.be/id/straatnaam/23489"
        };
    }
}
