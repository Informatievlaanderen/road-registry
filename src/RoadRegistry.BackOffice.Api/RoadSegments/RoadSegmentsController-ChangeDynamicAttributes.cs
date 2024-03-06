namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Abstractions.RoadSegments;
using BackOffice.Handlers.Sqs.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using ChangeDynamicAttributes;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValidationException = FluentValidation.ValidationException;

public partial class RoadSegmentsController
{
    private const string ChangeDynamicAttributesRoute = "acties/wijzigen/dynamischeattributen";

    /// <summary>
    ///     Wijzig een dynamisch attribuut voor één of meerdere wegsegmenten.
    /// </summary>
    /// <param name="parameters">Bevat de dynamische attributen die gewijzigd moeten worden</param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(ChangeDynamicAttributesRoute, Name = nameof(ChangeDynamicAttributes))]
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
    [SwaggerRequestExample(typeof(ChangeRoadSegmentsDynamicAttributesParameters), typeof(ChangeRoadSegmentsDynamicAttributesParametersExamples))]
    [SwaggerOperation(
        OperationId = nameof(ChangeDynamicAttributes),
        Description = "Dynamische attributen wijzigen van een wegsegment: wegverharding, wegbreedte en aantal rijstroken."
    )]
    public async Task<IActionResult> ChangeDynamicAttributes(
        [FromBody] ChangeRoadSegmentsDynamicAttributesParameters parameters,
        [FromServices] ChangeRoadSegmentsDynamicAttributesParametersValidator validator,
        CancellationToken cancellationToken)
    {
        try
        {
            var validatorResult = await validator.ValidateAsync(parameters, cancellationToken);
            if (!validatorResult.IsValid)
            {
                validatorResult.Errors.ForEach(error =>
                {
                    var bracketIndex = error.PropertyName.IndexOf('[');
                    if (bracketIndex != -1)
                    {
                        error.PropertyName = error.PropertyName.Substring(bracketIndex);
                    }
                });
                throw new ValidationException(validatorResult.Errors);
            }

            var request = TranslateParametersIntoTypedBackOfficeRequest();

            var sqsRequest = new ChangeRoadSegmentsDynamicAttributesSqsRequest
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

        ChangeRoadSegmentsDynamicAttributesRequest TranslateParametersIntoTypedBackOfficeRequest()
        {
            ChangeRoadSegmentsDynamicAttributesRequest attributesRequest = new();

            foreach (var attributesChange in parameters)
            {
                var roadSegmentId = new RoadSegmentId(attributesChange.WegsegmentId!.Value);
                attributesRequest.Add(roadSegmentId, roadSegment =>
                {
                    if (attributesChange.Wegverharding?.Length > 0)
                    {
                        roadSegment.Surfaces = attributesChange.Wegverharding
                            .Select(x => new ChangeRoadSegmentSurfaceAttributeRequest
                            {
                                FromPosition = new RoadSegmentPosition(x.VanPositie!.Value),
                                ToPosition = new RoadSegmentPosition(x.TotPositie!.Value),
                                Type = RoadSegmentSurfaceType.ParseUsingDutchName(x.Type)
                            })
                            .ToArray();
                    }

                    if (attributesChange.Wegbreedte?.Length > 0)
                    {
                        roadSegment.Widths = attributesChange.Wegbreedte
                            .Select(x => new ChangeRoadSegmentWidthAttributeRequest
                            {
                                FromPosition = new RoadSegmentPosition(x.VanPositie!.Value),
                                ToPosition = new RoadSegmentPosition(x.TotPositie!.Value),
                                Width = RoadSegmentWidth.ParseUsingDutchName(x.Breedte)
                            })
                            .ToArray();
                    }

                    if (attributesChange.AantalRijstroken?.Length > 0)
                    {
                        roadSegment.Lanes = attributesChange.AantalRijstroken
                            .Select(x => new ChangeRoadSegmentLaneAttributeRequest
                            {
                                FromPosition = new RoadSegmentPosition(x.VanPositie!.Value),
                                ToPosition = new RoadSegmentPosition(x.TotPositie!.Value),
                                Count = RoadSegmentLaneCount.ParseUsingDutchName(x.Aantal),
                                Direction = RoadSegmentLaneDirection.ParseUsingDutchName(x.Richting)
                            })
                            .ToArray();
                    }
                });
            }

            return attributesRequest;
        }
    }
}
