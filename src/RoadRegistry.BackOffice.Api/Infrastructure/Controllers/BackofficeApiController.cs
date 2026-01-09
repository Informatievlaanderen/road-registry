namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Globalization;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Extensions;
using Hosts.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public abstract class BackofficeApiController : ApiController
{
    protected BackofficeApiControllerContext ApiContext { get; }

    protected BackofficeApiController()
    {
    }

    protected BackofficeApiController(BackofficeApiControllerContext apiContext)
    {
        ApiContext = apiContext;
    }

    protected IActionResult Accepted(LocationResult locationResult, object? value = null)
    {
        return Accepted(locationResult
            .Location
            .ToString()
            .Replace(ApiContext.TicketingOptions.InternalBaseUrl, ApiContext.TicketingOptions.PublicBaseUrl), value);
    }

    protected void AddHeaderRetryAfter(int retryAfter)
    {
        if (retryAfter > 0)
        {
            Response.Headers["Retry-After"] = retryAfter.ToString(CultureInfo.InvariantCulture);
        }
    }

    protected ProvenanceData CreateProvenanceData(Modification modification = Modification.Unknown)
    {
        return new RoadRegistryProvenanceData(modification, ApiContext.HttpContextAccessor.HttpContext?.GetOperator());
    }
}

public class BackofficeApiControllerContext
{
    public BackofficeApiControllerContext(TicketingOptions ticketingOptions, IHttpContextAccessor httpContextAccessor)
    {
        TicketingOptions = ticketingOptions;
        HttpContextAccessor = httpContextAccessor;
    }

    public TicketingOptions TicketingOptions { get; }
    public IHttpContextAccessor HttpContextAccessor { get; }
}
