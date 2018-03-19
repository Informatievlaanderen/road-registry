namespace RoadRegistry.Api.Oslo.Infrastructure.ETag
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    public class PreconditionFailedResult : ActionResult
    {
        public override void ExecuteResult(ActionContext context)
        {
            base.ExecuteResult(context);

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.HttpContext.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
            context.HttpContext.Response.Headers.Add(HeaderNames.RetryAfter, "1");
        }
    }
}
