namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure;
using Infrastructure.Authentication;
using Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string DeleteRoadSegmentsRoute = "acties/verwijderen";

    /// <summary>
    ///     Verwijder wegsegmenten.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als de wegsegmenten gevonden zijn.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(DeleteRoadSegmentsRoute, Name = nameof(DeleteRoadSegments))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(DeleteRoadSegments), Description = "Verwijder wegsegmenten")]
    public async Task<IActionResult> DeleteRoadSegments(
        [FromBody] DeleteRoadSegmentsParameters parameters,
        [FromServices] DeleteRoadSegmentsParametersValidator validator,
        CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            var request = new DeleteRoadSegmentsRequest(parameters.Wegsegmenten.Select(x => new RoadSegmentId(x)).ToArray());
            var response = await _mediator.Send(request, cancellationToken);

            return Accepted(new LocationResult(response.TicketUrl));
        }
        catch (RoadSegmentsNotFoundException ex)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = nameof(parameters.Wegsegmenten),
                    ErrorCode = ProblemCode.RoadSegments.NotFound,
                    CustomState = new[]
                    {
                        new ProblemParameter("RoadSegmentIds", string.Join(",", ex.RoadSegmentIds))
                    }
                }
            ]);
        }
    }
}

[DataContract(Name = "WegsegmentenVerwijderen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentenVerwijderen")]
public class DeleteRoadSegmentsParameters
{
    /// <summary>
    ///     Lijst van identificatoren van wegsegmenten die verwijderd mogen worden.
    /// </summary>
    [DataMember(Name = "Wegsegmenten", Order = 1)]
    [JsonProperty("wegsegmenten", Required = Required.Always)]
    public int[] Wegsegmenten { get; set; }
}

public class DeleteRoadSegmentsParametersValidator : AbstractValidator<DeleteRoadSegmentsParameters>
{
    public DeleteRoadSegmentsParametersValidator()
    {
        RuleFor(x => x.Wegsegmenten)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .Must(wegsegmenten => wegsegmenten.All(RoadSegmentId.Accepts))
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .Must(x => x.Length == x.Distinct().Count())
            .WithProblemCode(ProblemCode.RoadSegment.IdsNotUnique);
    }
}
