namespace RoadRegistry.BackOffice.Api.Configuration;

using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Core;

public class ExceptionTranslationMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionTranslationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        var task = _next(context);

        if (!task.IsFaulted) return task;
        var ex = task.Exception.InnerExceptions[0] switch
        {
            ValidationException validationException => validationException.TranslateToDutch(),
            AggregateIdIsNotFoundException => new ApiException(new RoadNetworkNotFound().TranslateToDutch().Message, StatusCodes.Status404NotFound),
            _ => task.Exception.InnerExceptions[0]
        };
        return Task.FromException(ex);
    }
}
