namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Core.ProblemCodes;
using Editor.Schema;
using Extensions;
using FeatureToggles;
using FluentValidation;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using ValidationException = FluentValidation.ValidationException;
using ValidationFailure = FluentValidation.Results.ValidationFailure;
using ValidationResult = FluentValidation.Results.ValidationResult;

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
            foreach (var roadSegmentId in attributesChange.Wegsegmenten.Select(roadSegmentId => new RoadSegmentId(roadSegmentId)))
            {
                attributesRequest.Add(roadSegmentId, roadSegment =>
                {
                    if (attributesChange.Wegbeheerder is not null)
                    {
                        roadSegment.MaintenanceAuthority = new OrganizationId(attributesChange.Wegbeheerder);
                    }

                    if (attributesChange.WegsegmentStatus is not null)
                    {
                        roadSegment.Status = RoadSegmentStatus.ParseUsingDutchName(attributesChange.WegsegmentStatus);
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

[DataContract(Name = "WegsegmentAttribuutWijzigen", Namespace = "")]
public class ChangeRoadSegmentAttributesParameters : List<ChangeAttributeParameters>
{
}

public class ChangeRoadSegmentAttributesParametersValidator : AbstractValidator<ChangeRoadSegmentAttributesParameters>
{
    protected override bool PreValidate(ValidationContext<ChangeRoadSegmentAttributesParameters> context, ValidationResult result)
    {
        if (context.InstanceToValidate is not null && context.InstanceToValidate.Any())
        {
            return true;
        }

        context.AddFailure(new ValidationFailure
        {
            PropertyName = "request",
            ErrorCode = ProblemCode.RoadSegment.ChangeAttributesRequestNull
        });

        return false;
    }
}

public class ChangeRoadSegmentAttributesParametersExamples : IExamplesProvider<ChangeRoadSegmentAttributesParameters>
{
    public ChangeRoadSegmentAttributesParameters GetExamples()
    {
        return new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { 481110, 481111 },
                Wegbeheerder = "AWV112"
            },
            new()
            {
                Wegsegmenten = new[] { 481111 },
                Wegbeheerder = "AWV114"
            },
            new()
            {
                Wegsegmenten = new[] { 481111 },
                WegsegmentStatus = "buiten gebruik"
            },
            new()
            {
                Wegsegmenten = new[] { 481111 },
                MorfologischeWegklasse = "aardeweg"
            }
        };
    }
}

public class ChangeRoadSegmentAttributesParametersWrapperValidator : AbstractValidator<ChangeRoadSegmentAttributesParametersWrapper>
{
    public ChangeRoadSegmentAttributesParametersWrapperValidator(EditorContext editorContext)
    {
        ChangeAttributeParametersValidator validator = new(editorContext);

        RuleForEach(x => x.Attributes).SetValidator(validator);
    }
}

public class ChangeRoadSegmentAttributesParametersWrapper
{
    public List<ChangeAttributeParameters> Attributes { get; set; }

    public static explicit operator ChangeRoadSegmentAttributesParametersWrapper(ChangeRoadSegmentAttributesParameters p)
    {
        return new ChangeRoadSegmentAttributesParametersWrapper { Attributes = p };
    }

    public static implicit operator ChangeRoadSegmentAttributesParameters(ChangeRoadSegmentAttributesParametersWrapper p)
    {
        return p.Attributes as ChangeRoadSegmentAttributesParameters;
    }
}

public record ChangeAttributeParameters
{
    /// <summary>
    ///     Lijst van identificatoren van wegsegmenten waarvoor de attributen moeten gewijzigd worden.
    /// </summary>
    [DataMember(Name = "Wegsegmenten", Order = 1)]
    [JsonProperty(Required = Required.Always)]
    public int[] Wegsegmenten { get; set; }

    /// <summary>
    ///     De organisatie die verantwoordelijk is voor het fysieke onderhoud en beheer van de weg op het terrein.
    /// </summary>
    [DataMember(Name = "Wegbeheerder", Order = 2)]
    [JsonProperty]
    public string Wegbeheerder { get; set; }

    /// <summary>
    ///     De status van het wegsegment.
    /// </summary>
    [DataMember(Name = "WegsegmentStatus", Order = 3)]
    [JsonProperty("wegsegmentstatus")]
    [EnumDataType(typeof(RoadSegmentStatus))]
    public string WegsegmentStatus { get; set; }

    /// <summary>
    ///     De wegklasse van het wegsegment.
    /// </summary>
    [DataMember(Name = "MorfologischeWegklasse", Order = 4)]
    [JsonProperty]
    [EnumDataType(typeof(RoadSegmentMorphology))]
    public string MorfologischeWegklasse { get; set; }

    /// <summary>
    ///     De toegankelijkheid van het wegsegment voor de weggebruiker.
    /// </summary>
    [DataMember(Name = "Toegangsbeperking", Order = 5)]
    [JsonProperty]
    [EnumDataType(typeof(RoadSegmentAccessRestriction))]
    public string Toegangsbeperking { get; set; }

    /// <summary>
    ///     De wegcategorie van het wegsegment.
    /// </summary>
    [DataMember(Name = "Wegcategorie", Order = 6)]
    [JsonProperty]
    [EnumDataType(typeof(RoadSegmentCategory))]
    public string Wegcategorie { get; set; }
}

public class ChangeAttributeParametersValidator : AbstractValidator<ChangeAttributeParameters>
{
    private readonly EditorContext _editorContext;

    protected override bool PreValidate(ValidationContext<ChangeAttributeParameters> context, ValidationResult result)
    {
        if (context.InstanceToValidate is not null
            && context.InstanceToValidate.MorfologischeWegklasse is null
            && context.InstanceToValidate.Toegangsbeperking is null
            && context.InstanceToValidate.Wegbeheerder is null
            && context.InstanceToValidate.Wegcategorie is null
            && context.InstanceToValidate.WegsegmentStatus is null
            )
        {
            context.AddFailure(new ValidationFailure
            {
                PropertyName = "attribuut",
                ErrorCode = ProblemCode.Common.JsonInvalid
            });

            return false;
        }

        return true;
    }

    public ChangeAttributeParametersValidator(EditorContext editorContext)
    {
        _editorContext = editorContext.ThrowIfNull();

        RuleFor(x => x.Wegsegmenten)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .Must(wegsegmenten => wegsegmenten.All(RoadSegmentId.Accepts))
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .MustAsync(BeExistingNonRemovedRoadSegment)
            .WithProblemCode(ProblemCode.RoadSegments.NotFound, wegsegmenten => string.Join(", ", FindNonExistingOrRemovedRoadSegmentIds(wegsegmenten)));

        When(x => x.Wegbeheerder is not null, () =>
        {
            RuleFor(x => x.Wegbeheerder)
                .Cascade(CascadeMode.Stop)
                .MustAsync(BeKnownOrganization)
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid);
        });

        When(x => x.WegsegmentStatus is not null, () =>
        {
            RuleFor(x => x.WegsegmentStatus)
                .Cascade(CascadeMode.Stop)
                .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value) != RoadSegmentStatus.Unknown)
                .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);
        });

        When(x => x.MorfologischeWegklasse is not null, () =>
        {
            RuleFor(x => x.MorfologischeWegklasse)
                .Cascade(CascadeMode.Stop)
                .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value) != RoadSegmentMorphology.Unknown)
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);
        });

        When(x => x.Toegangsbeperking is not null, () =>
        {
            RuleFor(x => x.Toegangsbeperking)
                .Cascade(CascadeMode.Stop)
                .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);
        });

        When(x => x.Wegcategorie is not null, () =>
        {
            RuleFor(x => x.Wegcategorie)
                .Cascade(CascadeMode.Stop)
                .Must(RoadSegmentCategory.CanParseUsingDutchName)
                .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid);
        });
    }

    private Task<bool> BeExistingNonRemovedRoadSegment(int[] ids, CancellationToken cancellationToken)
    {
        return Task.FromResult(!FindNonExistingOrRemovedRoadSegmentIds(ids).Any());
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        return _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);
    }

    private IEnumerable<int> FindExistingAndNonRemovedRoadSegmentIds(IEnumerable<int> ids)
    {
        return _editorContext.RoadSegments.Select(s => s.Id).Where(w => ids.Contains(w));
    }

    private IEnumerable<int> FindNonExistingOrRemovedRoadSegmentIds(ICollection<int> ids)
    {
        return ids.Except(FindExistingAndNonRemovedRoadSegmentIds(ids));
    }
}
