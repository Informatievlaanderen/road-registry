namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using System.Globalization;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FeatureToggle;
using Hosts.Infrastructure.Options;
using Microsoft.AspNetCore.Mvc;

public abstract class BackofficeApiController : ApiController
{
    private readonly TicketingOptions _ticketingOptions;

    protected BackofficeApiController()
    {
    }

    protected BackofficeApiController(TicketingOptions ticketingOptions)
    {
        _ticketingOptions = ticketingOptions;
    }

    protected IActionResult Accepted(LocationResult locationResult)
    {
        return Accepted(locationResult
            .Location
            .ToString()
            .Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl));
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
}
