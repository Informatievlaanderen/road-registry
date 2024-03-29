namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using System.Threading.Tasks;
using Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

[AttributeUsage(AttributeTargets.Class)]
public class SupportUrlFormatAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        context.SetContentFormatAcceptType();

        await next();
    }
}