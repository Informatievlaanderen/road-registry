namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using FeatureToggles;
using FluentValidation;
using FluentValidation.Results;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parameters;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

public partial class RoadSegmentsController
{
    /// <summary>
    ///     Attribuutwaarde van status, toegangsbeperking, wegklasse, wegbeheerder en wegcategorie van wegsegmenten wijzigen.
    /// </summary>
    /// <param name="featureToggle">Ingeschakelde functionaliteit of niet</param>
    /// <param name="parameters">Bevat de attributen die gewijzigd moeten worden</param>
    /// <param name="validator"></param>
    /// <param name="wrappedValidator"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost("acties/attributenwijzigen")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.WegenAttribuutWaarden.Beheerder)]
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
    [SwaggerOperation(Description = "Attributen wijzigen van een wegsegment: status, toegangsbeperking, wegklasse, wegbeheerder en wegcategorie.")]
    public async Task<IActionResult> PostChangeAttributes(
        [FromServices] UseRoadSegmentChangeAttributesFeatureToggle featureToggle,
        [FromBody] ChangeRoadSegmentAttributesParameters parameters,
        [FromServices] ChangeRoadSegmentAttributesParametersValidator validator,
        [FromServices] ChangeRoadSegmentAttributesParametersWrapperValidator wrappedValidator,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            var wrappedValidatorResult = await wrappedValidator.ValidateAsync((ChangeRoadSegmentAttributesParametersWrapper)parameters, cancellationToken);
            if (!wrappedValidatorResult.IsValid)
            {
                wrappedValidatorResult.Errors.ForEach(error => error.PropertyName = error.PropertyName.Replace($"{nameof(ChangeRoadSegmentAttributesParametersWrapper.Attributes)}[", "["));
                throw new ValidationException(wrappedValidatorResult.Errors);
            }

            var request = TranslateParametersIntoTypedBackOfficeRequest();

            var sqsRequest = new ChangeRoadSegmentAttributesSqsRequest
            {
                Request = request
            };

            var result = await _mediator.Send(Enrich(sqsRequest), cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }

        ChangeRoadSegmentAttributesRequest TranslateParametersIntoTypedBackOfficeRequest()
        {
            ChangeRoadSegmentAttributesRequest attributesRequest = new();

            foreach (var attributesChange in parameters)
            {
                var attributeEnum = Enum.Parse<ChangeRoadSegmentAttribute>(attributesChange.Attribuut, true);

                switch (attributeEnum)
                {
                    case ChangeRoadSegmentAttribute.Wegbeheerder:
                        foreach (var roadSegmentId in attributesChange.Wegsegmenten)
                        {
                            attributesRequest.Add(new RoadSegmentId(roadSegmentId), roadSegment
                                => roadSegment.MaintenanceAuthority = new OrganizationId(attributesChange.Attribuutwaarde)
                            );
                        }
                        break;
                    case ChangeRoadSegmentAttribute.WegsegmentStatus:
                        foreach (var roadSegmentId in attributesChange.Wegsegmenten)
                        {
                            attributesRequest.Add(new RoadSegmentId(roadSegmentId), roadSegment
                                => roadSegment.Status = RoadSegmentStatus.ParseUsingDutchName(attributesChange.Attribuutwaarde)
                            );
                        }
                        break;
                    case ChangeRoadSegmentAttribute.MorfologischeWegklasse:
                        foreach (var roadSegmentId in attributesChange.Wegsegmenten)
                        {
                            attributesRequest.Add(new RoadSegmentId(roadSegmentId), roadSegment
                                => roadSegment.Morphology = RoadSegmentMorphology.ParseUsingDutchName(attributesChange.Attribuutwaarde)
                            );
                        }
                        break;
                    case ChangeRoadSegmentAttribute.Toegangsbeperking:
                        foreach (var roadSegmentId in attributesChange.Wegsegmenten)
                        {
                            attributesRequest.Add(new RoadSegmentId(roadSegmentId), roadSegment
                                => roadSegment.AccessRestriction = RoadSegmentAccessRestriction.ParseUsingDutchName(attributesChange.Attribuutwaarde)
                            );
                        }
                        break;
                    case ChangeRoadSegmentAttribute.Wegcategorie:
                        foreach (var roadSegmentId in attributesChange.Wegsegmenten)
                        {
                            attributesRequest.Add(new RoadSegmentId(roadSegmentId), roadSegment
                                => roadSegment.Category = RoadSegmentCategory.ParseUsingDutchName(attributesChange.Attribuutwaarde)
                            );
                        }
                        break;
                    default:
                        throw new ValidationException("request", new[] { new ValidationFailure("request", "Onbehandelde waarde voor attribuut") });
                }
            }

            return attributesRequest;
        }
    }
}
