namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Abstractions.RoadSegmentsOutline;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
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
    ///     Attribuutwaarde van status, toegangsbeperking, wegklasse, wegbeheerder en wegcategorie van wegsegmenten wijzigen.
    /// </summary>
    /// <param name="featureToggle">Ingeschakelde functionaliteit of niet</param>
    /// <param name="id">Identificator van het wegsegment.</param>
    /// <param name="parameters">Bevat de attributen die gewijzigd moeten worden</param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost("{id}/acties/attribuutwijzigen")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", "string", "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", "string", "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(ChangeRoadSegmentAttributesParameters), typeof(ChangeRoadSegmentAttributesParametersExamples))]
    [SwaggerOperation(Description = "Attribuutwaarde van status, toegangsbeperking, wegklasse, wegbeheerder en wegcategorie van wegsegmenten wijzigen.")]
    public async Task<IActionResult> PostChangeAttributes(
        [FromServices] UseRoadSegmentChangeAttributesFeatureToggle featureToggle,
        [FromRoute] int id,
        [FromBody] ChangeRoadSegmentAttributesParameters parameters,
        [FromServices] ChangeRoadSegmentAttributesParametersValidator validator,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            var sqsRequest = new ChangeRoadSegmentAttributesSqsRequest
                {
                    Request = new ChangeRoadSegmentAttributesRequest(
                        id,
                        parameters.Wegsegmentstatus is not null ? RoadSegmentStatus.ParseUsingDutchName(parameters.Wegsegmentstatus) : null,
                        parameters.MorfologischeWegklasse is not null ? RoadSegmentMorphology.ParseUsingDutchName(parameters.MorfologischeWegklasse) : null,
                        parameters.Toegangsbeperking is not null ? RoadSegmentAccessRestriction.ParseUsingDutchName(parameters.Toegangsbeperking) : null,
                        parameters.Wegbeheerder is not null ? new OrganizationId(parameters.Wegbeheerder) : null,
                        parameters.Wegcategorie is not null ? RoadSegmentCategory.ParseUsingDutchName(parameters.Wegcategorie) : null
                    )
            };

            var result = await _mediator.Send(Enrich(sqsRequest), cancellationToken);

            return Accepted(result);
        }
        catch (AggregateIdIsNotFoundException)
        {
            throw new ApiException(ValidationErrors.RoadNetwork.NotFound.Message, StatusCodes.Status404NotFound);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}
