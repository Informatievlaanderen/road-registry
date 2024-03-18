namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using Abstractions.Exceptions;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Core;
using Core.ProblemCodes;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using ValidationProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ValidationProblemDetails;

internal class ValidationFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        switch (context.Exception)
        {
            case DutchValidationException:
                break;
            case ValidationException validationException:
                context.Exception = validationException.TranslateToDutch();
                break;
            case AggregateIdIsNotFoundException:
                context.Exception = new ApiException(new RoadNetworkNotFound().TranslateToDutch().Message, StatusCodes.Status404NotFound);
                break;
            case RoadSegmentOutlinedNotFoundException:
                context.Exception = new ApiException(new RoadSegmentOutlinedNotFound().TranslateToDutch().Message, StatusCodes.Status404NotFound);
                break;
            case RoadSegmentNotFoundException:
                context.Exception = new ApiException(new RoadSegmentNotFound().TranslateToDutch().Message, StatusCodes.Status404NotFound);
                break;
            case ZipArchiveValidationException zipArchiveValidationException:
                context.Exception = zipArchiveValidationException.ToDutchValidationException();
                break;
        }
    }

    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ModelState.IsValid)
        {
            return base.OnActionExecutionAsync(context, next);
        }

        context.Result = new BadRequestObjectResult(
            new ValidationProblemDetails(
                new ValidationException(new[] { new ValidationFailure { PropertyName = "request", ErrorCode = ProblemCode.Common.JsonInvalid } }).TranslateToDutch()));
        return Task.CompletedTask;
    }
}
