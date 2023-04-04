namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Core;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

internal class ValidationFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        try
        {
            context.Exception = context.Exception switch
            {
                ValidationException validationException => validationException.TranslateToDutch(),
                AggregateIdIsNotFoundException => new ApiException(new RoadNetworkNotFound().TranslateToDutch().Message, StatusCodes.Status404NotFound),
                _ => context.Exception
            };
        }
        catch
        {
            // This needs to remain because this block will be called twice due to the exception manipulation
        }
    }
}
