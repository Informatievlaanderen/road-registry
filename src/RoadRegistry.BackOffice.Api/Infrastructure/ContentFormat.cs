namespace RoadRegistry.BackOffice.Api.Infrastructure;

using System;
using Be.Vlaanderen.Basisregisters.Api;
using Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class ContentFormat
{
    private ContentFormat(AcceptType contentType)
    {
        ContentType = contentType;
    }

    public AcceptType ContentType { get; }

    public static AcceptType? DetermineAcceptType(ActionContext? context)
    {
        return context?.HttpContext.Request
            .GetTypedHeaders()
            .DetermineAcceptType(context.ActionDescriptor);
    }

    public static ContentFormat For(
        EndpointType endpointType,
        ActionContext? context)
    {
        var acceptType = DetermineAcceptType(context)
            ?.ValidateFor(endpointType);

        return new ContentFormat(acceptType ?? throw new InvalidOperationException("Invalid accept type."));
    }
}
