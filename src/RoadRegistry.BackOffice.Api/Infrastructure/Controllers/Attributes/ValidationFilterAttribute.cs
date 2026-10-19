namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using Abstractions.Exceptions;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Core;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandHandling;
using RoadRegistry.Extracts.DutchTranslations;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using ValueObjects.ProblemCodes;
using ValueObjects.Problems;
using ValidationProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ValidationProblemDetails;

internal class ValidationFilterAttribute : ActionFilterAttribute
{
    // A parameter marked [JsonProperty(Required = Required.Always)] is enforced by Newtonsoft while the body is
    // bound, so an action - and the validation it runs - never sees a request that leaves one out or sends it as
    // null. Which parameter is missing only lives in Newtonsoft's own message, hence this pattern; the exact
    // wording it has to keep matching is pinned by the tests against the messages Newtonsoft actually produces.
    private static readonly Regex RequiredParameterPattern = new(
        @"^Required property '(?<parameter>[^']+)' (?:not found in JSON|expects a value but got null|expects a non-null value)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        switch (context.Exception)
        {
            case DutchValidationException:
                break;
            case ValidationException validationException:
                context.Exception = validationException.TranslateToDutch(WellKnownProblemTranslators.Default);
                break;
            case AggregateIdIsNotFoundException:
                context.Exception = new ApiException(new RoadNetworkNotFound().TranslateToDutch(WellKnownProblemTranslators.Default).Message, StatusCodes.Status404NotFound);
                break;
            case RoadSegmentOutlinedNotFoundException:
                context.Exception = new ApiException(new RoadSegmentOutlinedNotFound().TranslateToDutch(WellKnownProblemTranslators.Default).Message, StatusCodes.Status404NotFound);
                break;
            case RoadSegmentNotFoundException:
                context.Exception = new ApiException(new RoadSegmentNotFound().TranslateToDutch(WellKnownProblemTranslators.Default).Message, StatusCodes.Status404NotFound);
                break;
            case ZipArchiveValidationException zipArchiveValidationException:
                context.Exception = zipArchiveValidationException.ToDutchValidationException(FileProblemTranslator.DomainV1);
                break;
        }
    }

    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ModelState.IsValid)
        {
            return base.OnActionExecutionAsync(context, next);
        }

        // A body that is unusable for any other reason stays a single, generic failure: nothing more specific than
        // 'this is not the JSON the endpoint takes' can be said about it.
        var failures = MissingRequiredParameterFailures(context.ModelState);
        if (failures.Count == 0)
        {
            failures = [new ValidationFailure { PropertyName = "request", ErrorCode = ProblemCode.Common.JsonInvalid }];
        }

        context.Result = new BadRequestObjectResult(
            new ValidationProblemDetails(
                new ValidationException(failures)
                    .TranslateToDutch(WellKnownProblemTranslators.Default)));
        return Task.CompletedTask;
    }

    // One failure per parameter the caller left out or sent as null, so a request that misses several is answered
    // about all of them at once. The model state key is the path to the parameter - 'wegsegmenten' at the root of
    // the body, '[0].wegsegmenten' inside an array - which is the same shape the controllers use when they report a
    // missing parameter themselves.
    private static List<ValidationFailure> MissingRequiredParameterFailures(ModelStateDictionary modelState)
    {
        return modelState
            .SelectMany(entry => entry.Value.Errors.Select(error => new
            {
                PropertyName = entry.Key,
                Match = RequiredParameterPattern.Match(error.ErrorMessage)
            }))
            .Where(x => x.Match.Success)
            .Select(x => new ValidationFailure(x.PropertyName, $"De parameter '{x.Match.Groups["parameter"].Value}' is verplicht."))
            .DistinctBy(x => (x.PropertyName, x.ErrorMessage))
            .ToList();
    }
}
