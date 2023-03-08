namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System;
using System.Collections.Generic;
using System.Linq;
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
using FluentValidation.Results;
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
    /// <param name="parameters">Bevat de attributen die gewijzigd moeten worden</param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost("acties/attributenwijzigen")]
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
            await wrappedValidator.ValidateAndThrowAsync((ChangeRoadSegmentAttributesParametersWrapper)parameters, cancellationToken);

            //var sqsRequest = new ChangeRoadSegmentAttributesSqsRequest
            //{
            //    Request = request
            //};

            //var result = await _mediator.Send(Enrich(sqsRequest), cancellationToken);

            return Accepted();
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

    //private static async IAsyncEnumerable<ChangeRoadSegmentAttributesRequest> ConvertRequestAsync(ChangeRoadSegmentAttributesParameters parameters)
    //{
    //    foreach (var attributesChange in parameters)
    //    {
    //        var attributeEnum = Enum.Parse<ChangeRoadSegmentAttributesEnum>(attributesChange.Attribuut, true);

    //        ChangeRoadSegmentAttributesRequest request = 

    //        switch (attributeEnum)
    //        {
    //            case ChangeRoadSegmentAttributesEnum.Wegbeheerder:
    //                break;
    //            case ChangeRoadSegmentAttributesEnum.WegsegmentStatus:
    //                break;
    //            case ChangeRoadSegmentAttributesEnum.MorfologischeWegklasse:
    //                break;
    //            case ChangeRoadSegmentAttributesEnum.Toegangsbeperking:
    //                break;
    //            case ChangeRoadSegmentAttributesEnum.Wegcategorie:
    //                break;
    //        }
    //        yield return request;
    //    }
    //}
}
