namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Core.ProblemCodes;
using Editor.Schema;
using Extensions;
using FluentValidation;
using Infrastructure;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    private const string DeleteBulkRoute = "acties/verwijderen";

    /// <summary>
    ///     Verwijder wegsegmenten.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als de wegsegmenten gevonden zijn.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost(DeleteBulkRoute, Name = nameof(DeleteBulk))]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes, Policy = PolicyNames.WegenUitzonderingen.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerOperation(OperationId = nameof(DeleteBulk), Description = "Verwijder wegsegmenten")]
    public async Task<IActionResult> DeleteBulk(
        [FromBody] PostDeleteRoadSegmentsParameters request,
        [FromServices] PostDeleteRoadSegmentsParametersValidator validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var response = await _mediator.Send(new DeleteRoadSegmentsRequest(request.Wegsegmenten), cancellationToken);

        return Accepted(new LocationResult(response.TicketUrl));
    }

    [DataContract(Name = "WegsegmentenVerwijderen", Namespace = "")]
    [CustomSwaggerSchemaId("WegsegmentenVerwijderen")]
    public class PostDeleteRoadSegmentsParameters
    {
        /// <summary>
        ///     Lijst van identificatoren van wegsegmenten die verwijderd mogen worden.
        /// </summary>
        [DataMember(Name = "Wegsegmenten", Order = 1)]
        [JsonProperty("wegsegmenten", Required = Required.Always)]
        public int[] Wegsegmenten { get; set; }
    }

    public class PostDeleteRoadSegmentsParametersValidator : AbstractValidator<PostDeleteRoadSegmentsParameters>
    {
        private readonly EditorContext _editorContext;

        public PostDeleteRoadSegmentsParametersValidator(EditorContext editorContext)
        {
            _editorContext = editorContext;

            RuleFor(x => x.Wegsegmenten)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithProblemCode(ProblemCode.Common.JsonInvalid)
                .Must(wegsegmenten => wegsegmenten.All(RoadSegmentId.Accepts))
                .WithProblemCode(ProblemCode.Common.JsonInvalid)
                .MustAsync(BeExistingNonRemovedRoadSegment)
                .WithProblemCode(ProblemCode.RoadSegments.NotFound, wegsegmenten => string.Join(", ", FindNonExistingOrRemovedRoadSegmentIds(wegsegmenten)));
        }

        private Task<bool> BeExistingNonRemovedRoadSegment(int[] ids, CancellationToken cancellationToken)
        {
            return Task.FromResult(!FindNonExistingOrRemovedRoadSegmentIds(ids).Any());
        }

        private IEnumerable<int> FindNonExistingOrRemovedRoadSegmentIds(ICollection<int> ids)
        {
            return ids.Except(FindExistingAndNonRemovedRoadSegmentIds(ids));
        }

        private IEnumerable<int> FindExistingAndNonRemovedRoadSegmentIds(IEnumerable<int> ids)
        {
            return _editorContext.RoadSegments.Select(s => s.Id).Where(w => ids.Contains(w));
        }
    }
}
