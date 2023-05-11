namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System;
using System.Collections.Generic;
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
using FluentValidation.Results;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

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

[DataContract(Name = "WegsegmentAttribuutWijzigen", Namespace = "")]
public class ChangeRoadSegmentAttributesParameters : List<ChangeAttributeParameters>
{
}

public class ChangeRoadSegmentAttributesParametersValidator : AbstractValidator<ChangeRoadSegmentAttributesParameters>
{
    /// <summary>
    ///     Determines if validation should occtur and provides a means to modify the context and ValidationResult prior to
    ///     execution.
    ///     If this method returns false, then the ValidationResult is immediately returned from Validate/ValidateAsync.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="result">The result.</param>
    /// <remarks>
    ///     The output for validation errors will not be correct if you do not fill in the property name!
    ///     You should not use the format noted inside the code below.
    ///     It's a valid exception but does not format correctly.
    /// </remarks>
    /// <code>
    /// context.AddFailure(new ValidationFailure
    /// {
    ///     ErrorCode = ValidationErrors.RoadSegment.ChangeAttributesRequestNull.Code,
    ///     ErrorMessage = ValidationErrors.RoadSegment.ChangeAttributesRequestNull.Message
    /// });
    /// </code>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
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
                Attribuut = "wegbeheerder",
                Attribuutwaarde = "AWV112",
                Wegsegmenten = new[] { 481110, 481111 }
            },
            new()
            {
                Attribuut = "wegbeheerder",
                Attribuutwaarde = "AWV114",
                Wegsegmenten = new[] { 481111 }
            },
            new()
            {
                Attribuut = "wegsegmentstatus",
                Attribuutwaarde = "buiten gebruik",
                Wegsegmenten = new[] { 481111 }
            },
            new()
            {
                Attribuut = "morfologischeWegklasse",
                Attribuutwaarde = "aardeweg",
                Wegsegmenten = new[] { 481111 }
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
        return new() { Attributes = p };
    }

    public static implicit operator ChangeRoadSegmentAttributesParameters(ChangeRoadSegmentAttributesParametersWrapper p)
    {
        return p.Attributes as ChangeRoadSegmentAttributesParameters;
    }
}

public record ChangeAttributeParameters
{
    /// <summary>
    ///     Het attribuut dat gewijzigd moet worden.
    /// </summary>
    [DataMember(Name = "attribuut", Order = 1)]
    [JsonProperty("attribuut")]
    public string Attribuut { get; set; }

    /// <summary>
    ///     De attribuutwaarde van het te wijzigen attribuut.
    /// </summary>
    [DataMember(Name = "attribuutwaarde", Order = 2)]
    [JsonProperty("attribuutwaarde")]
    public string Attribuutwaarde { get; set; }

    /// <summary>
    ///     Lijst van identificatoren van wegsegmenten waarvoor het attribuut moet gewijzigd worden.
    /// </summary>
    [DataMember(Name = "wegsegmenten", Order = 3)]
    [JsonProperty("wegsegmenten")]
    public int[] Wegsegmenten { get; set; }
}

public class ChangeAttributeParametersValidator : AbstractValidator<ChangeAttributeParameters>
{
    private readonly EditorContext _editorContext;

    public ChangeAttributeParametersValidator(EditorContext editorContext)
    {
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));

        RuleFor(x => x.Attribuut)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .IsEnumName(typeof(ChangeRoadSegmentAttribute), false)
            .WithProblemCode(ProblemCode.RoadSegment.ChangeAttributesAttributeNotValid);

        RuleFor(x => x.Attribuutwaarde)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Common.JsonInvalid);

        RuleFor(x => x.Wegsegmenten)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .Must(wegsegmenten => wegsegmenten.All(RoadSegmentId.Accepts))
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .MustAsync(BeExistingNonRemovedRoadSegment)
            .WithProblemCode(ProblemCode.RoadSegments.NotFound, wegsegmenten => string.Join(", ", FindNonExistingOrRemovedRoadSegmentIds(wegsegmenten)));

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.Wegbeheerder), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.IsRequired)
                .MustAsync(BeKnownOrganization)
                .WithProblemCode(ProblemCode.RoadSegment.MaintenanceAuthority.NotValid);
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.WegsegmentStatus), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Status.IsRequired)
                .Must(value => RoadSegmentStatus.CanParseUsingDutchName(value) && RoadSegmentStatus.ParseUsingDutchName(value) != RoadSegmentStatus.Unknown)
                .WithProblemCode(ProblemCode.RoadSegment.Status.NotValid);
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.MorfologischeWegklasse), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.IsRequired)
                .Must(value => RoadSegmentMorphology.CanParseUsingDutchName(value) && RoadSegmentMorphology.ParseUsingDutchName(value) != RoadSegmentMorphology.Unknown)
                .WithProblemCode(ProblemCode.RoadSegment.Morphology.NotValid);
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.Toegangsbeperking), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.IsRequired)
                .Must(RoadSegmentAccessRestriction.CanParseUsingDutchName)
                .WithProblemCode(ProblemCode.RoadSegment.AccessRestriction.NotValid);
        });

        When(x => IsEnum(x.Attribuut, ChangeRoadSegmentAttribute.Wegcategorie), () =>
        {
            RuleFor(x => x.Attribuutwaarde)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithProblemCode(ProblemCode.RoadSegment.Category.IsRequired)
                .Must(RoadSegmentCategory.CanParseUsingDutchName)
                .WithProblemCode(ProblemCode.RoadSegment.Category.NotValid);
        });
    }

    private Task<bool> BeExistingNonRemovedRoadSegment(int[] ids, CancellationToken cancellationToken)
    {
        return Task.FromResult(!FindNonExistingOrRemovedRoadSegmentIds(ids.AsEnumerable()).Any());
    }

    private Task<bool> BeKnownOrganization(string code, CancellationToken cancellationToken)
    {
        return _editorContext.Organizations.AnyAsync(x => x.Code == code, cancellationToken);
    }

    private IEnumerable<int> FindExistingAndNonRemovedRoadSegmentIds(IEnumerable<int> ids)
    {
        return _editorContext.RoadSegments.Select(s => s.Id).Where(w => ids.Contains(w));
    }

    private IEnumerable<int> FindNonExistingOrRemovedRoadSegmentIds(IEnumerable<int> ids)
    {
        return ids.Except(FindExistingAndNonRemovedRoadSegmentIds(ids));
    }

    private static bool IsEnum(string value, ChangeRoadSegmentAttribute enumMatch)
    {
        return Enum.TryParse(value, true, out ChangeRoadSegmentAttribute enumValue) && enumValue == enumMatch;
    }
}
