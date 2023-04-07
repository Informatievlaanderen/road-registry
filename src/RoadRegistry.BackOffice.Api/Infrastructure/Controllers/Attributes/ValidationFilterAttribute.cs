namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Core;
using Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

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
        }
    }
}
