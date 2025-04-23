namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Globalization;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Extensions;
using FeatureToggle;
using Hosts.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public abstract class BackofficeApiController : ApiController
{
    private readonly BackofficeApiControllerContext _context;

    protected BackofficeApiController()
    {
    }

    protected BackofficeApiController(BackofficeApiControllerContext context)
    {
        _context = context;
    }

    protected IActionResult Accepted(LocationResult locationResult)
    {
        return Accepted(locationResult
            .Location
            .ToString()
            .Replace(_context.TicketingOptions.InternalBaseUrl, _context.TicketingOptions.PublicBaseUrl));
    }

    protected void AddHeaderRetryAfter(int retryAfter)
    {
        if (retryAfter > 0)
        {
            Response.Headers.Add("Retry-After", retryAfter.ToString(CultureInfo.InvariantCulture));
        }
    }

    protected bool GetFeatureToggleValue<T>(T featureToggle)
        where T : IFeatureToggle
    {
        var headerKey = $"X-{typeof(T).Name}";
        if (Request.Headers.TryGetValue(headerKey, out var values))
        {
            var value = values.First();
            if (string.IsNullOrEmpty(value))
            {
                value = true.ToString();
            }

            if (bool.TryParse(value, out var featureEnabled))
            {
                return featureEnabled;
            }
        }

        return featureToggle.FeatureEnabled;
    }

    protected ProvenanceData CreateProvenanceData(Modification modification = Modification.Unknown)
    {
        return new RoadRegistryProvenanceData(modification, _context.HttpContextAccessor.HttpContext!.GetOperatorName());
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
