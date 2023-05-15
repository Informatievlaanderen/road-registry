namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using Be.Vlaanderen.Basisregisters.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

public static class HttpRequestExtensions
{
    public static string GetValueFromHeader(this ActionContext context, string key)
    {
        return context.HttpContext.Request.Headers.TryGetValue(key, out var headerValue) ? headerValue.ToString().ToLowerInvariant() : null;
    }

    public static string GetValueFromQueryString(this ActionContext context, string key)
    {
        return context.HttpContext.Request.Query.TryGetValue(key, out var queryValue) ? queryValue.ToString().ToLowerInvariant() : null;
    }

    public static string GetValueFromRouteData(this ActionContext context, string key)
    {
        if (!context.RouteData.Values.TryGetValue(key, out var value))
        {
            return null;
        }

        var routeValue = value?.ToString();

        return string.IsNullOrEmpty(routeValue) ? null : routeValue.ToLowerInvariant();
    }

    public static void RewriteAcceptTypeForProblemDetail(this HttpRequest request)
    {
        // convert Atom to Xml to support problem-details
        if (request.GetTypedHeaders().Contains(AcceptType.Atom))
        {
            request.SetAcceptType(AcceptType.Xml);
        }
    }

    public static void SetAcceptType(this HttpRequest request, AcceptType? acceptType)
    {
        request.Headers[HeaderNames.Accept] = acceptType?.ToMimeTypeString();
    }

    public static void SetContentFormatAcceptType(this ActionContext context)
    {
        context
            .HttpContext
            .Request
            .SetAcceptType(ContentFormat.DetermineAcceptType(context));
    }
}