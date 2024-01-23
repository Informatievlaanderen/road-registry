namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using ChangeAttributes;
using FeatureToggles;
using FluentValidation;
using Handlers.Sqs.RoadSegments;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using ValidationException = FluentValidation.ValidationException;

public partial class RoadSegmentsController
{
    private const string ChangeAttributesRoute = "acties/wijzigen/attributen";

    /// <summary>
    ///     Wijzig een attribuutwaarde voor één of meerdere wegsegmenten.
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
    [HttpPost(ChangeAttributesRoute, Name = nameof(ChangeAttributes))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenAttribuutWaarden.Beheerder)]
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
    [SwaggerOperation(OperationId = nameof(ChangeAttributes), Description = "Attributen wijzigen van een wegsegment: status, toegangsbeperking, wegklasse, wegbeheerder en wegcategorie.")]
    public async Task<IActionResult> ChangeAttributes(
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

            var result = await _mediator.Send(sqsRequest, cancellationToken);

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
            foreach (var roadSegmentId in attributesChange.Wegsegmenten.Select(roadSegmentId => new RoadSegmentId(roadSegmentId)))
            {
                attributesRequest.Add(roadSegmentId, roadSegment =>
                {
                    if (attributesChange.Wegbeheerder is not null)
                    {
                        roadSegment.MaintenanceAuthority = new OrganizationId(attributesChange.Wegbeheerder);
                    }

                    if (attributesChange.Wegsegmentstatus is not null)
                    {
                        roadSegment.Status = RoadSegmentStatus.ParseUsingDutchName(attributesChange.Wegsegmentstatus);
                    }

                    if (attributesChange.MorfologischeWegklasse is not null)
                    {
                        roadSegment.Morphology = RoadSegmentMorphology.ParseUsingDutchName(attributesChange.MorfologischeWegklasse);
                    }

                    if (attributesChange.Toegangsbeperking is not null)
                    {
                        roadSegment.AccessRestriction = RoadSegmentAccessRestriction.ParseUsingDutchName(attributesChange.Toegangsbeperking);
                    }

                    if (attributesChange.Wegcategorie is not null)
                    {
                        roadSegment.Category = RoadSegmentCategory.ParseUsingDutchName(attributesChange.Wegcategorie);
                    }
                });
            }

            return attributesRequest;
        }
    }
}
