namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegmentsOutline;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Dbase;
using Editor.Schema;
using FeatureToggles;
using FluentValidation;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parameters;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    /// <summary>
    ///     Maak een schets van een wegsegment
    /// </summary>
    /// <param name="featureToggle"></param>
    /// <param name="validator"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost("acties/schetsen")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", "string", "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", "string", "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(PostRoadSegmentOutlineParameters), typeof(PostRoadSegmentOutlineParametersExamples))]
    [SwaggerOperation(Description = "Nieuw wegsegment schetsen.")]
    public async Task<IActionResult> PostCreateOutline(
        [FromServices] UseRoadSegmentOutlineFeatureToggle featureToggle,
        [FromServices] PostRoadSegmentOutlineParametersValidator validator,
        [FromBody] PostRoadSegmentOutlineParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);
            
            var sqsRequest = new CreateRoadSegmentOutlineSqsRequest
            {
                Request = new CreateRoadSegmentOutlineRequest(
                    GeometryTranslator.ParseGmlLineString(parameters.MiddellijnGeometrie),
                    RoadSegmentStatus.ParseUsingDutchName(parameters.Wegsegmentstatus),
                    RoadSegmentMorphology.ParseUsingDutchName(parameters.MorfologischeWegklasse),
                    RoadSegmentAccessRestriction.ParseUsingDutchName(parameters.Toegangsbeperking),
                    new OrganizationId(parameters.Wegbeheerder),
                    RoadSegmentSurfaceType.ParseUsingDutchName(parameters.Wegverharding),
                    new RoadSegmentWidth(parameters.Wegbreedte),
                    new RoadSegmentLaneCount(parameters.AantalRijstroken.Aantal),
                    RoadSegmentLaneDirection.ParseUsingDutchName(parameters.AantalRijstroken.Richting)
                ),
                Metadata = GetMetadata(),
                ProvenanceData = new ProvenanceData(CreateFakeProvenance())
            };
            var result = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(result);
        }
        catch (AggregateIdIsNotFoundException)
        {
            throw new ApiException(ValidationErrors.Common.IncorrectObjectId.Message(RoadNetwork.Identifier), StatusCodes.Status404NotFound);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}
