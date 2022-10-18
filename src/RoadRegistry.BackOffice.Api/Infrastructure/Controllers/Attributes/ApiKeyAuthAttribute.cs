namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Extensions;
using FeatureToggles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _requiredAccess;

    public ApiKeyAuthAttribute(string requiredAccess)
    {
        _requiredAccess = requiredAccess;
    }

    private const string ApiKeyHeaderName = "x-api-key";
    private const string ApiKeyQueryName = "apikey";

    internal record ApiToken([JsonProperty("clientname")] string ClientName, [JsonProperty("apikey")] string ApiKey, [JsonProperty("metadata")] ApiTokenMetadata Metadata);

    private const string ApiTokenHeaderName = "x-api-token";

    internal record ApiTokenMetadata([JsonProperty("wraccess")] bool WrAccess, [JsonProperty("syncaccess")] bool SyncAccess, [JsonProperty("ticketsaccess")] bool TicketsAccess = false);

    public Task OnActionExecutionApiKeyAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sp = context.HttpContext.RequestServices;
        var useApiKeyAuthenticationToggle = sp.GetRequiredService<UseApiKeyAuthenticationToggle>();

        if (!useApiKeyAuthenticationToggle.FeatureEnabled) return next();

        var potentialQueryApiKey = StringValues.Empty;
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialHeaderApiKey)
            && !context.HttpContext.Request.Query.TryGetValue(ApiKeyQueryName, out potentialQueryApiKey))
            RefuseAccess(context, "API key verplicht.");

        var potentialApiKey = string.Empty;
        if (!string.IsNullOrWhiteSpace(potentialQueryApiKey)) potentialApiKey = potentialQueryApiKey;

        if (!string.IsNullOrWhiteSpace(potentialHeaderApiKey)) potentialApiKey = potentialHeaderApiKey;

        if (string.IsNullOrWhiteSpace(potentialApiKey)) RefuseAccess(context, "API key verplicht.");

        var validApiKeys = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IConfiguration>()
            .GetSection($"ApiKeys:{_requiredAccess}")
            .GetChildren()
            .Select(c => c.Value)
            .ToArray();

        if (!validApiKeys.Contains(potentialApiKey)) RefuseAccess(context, "Ongeldige API key.");

        return next();
    }

    public Task OnActionExecutionApiTokenAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // We get the x-api-key header or query param string
        // Check if the user used this and thus is not anonymous GAWR-2968
        var potentialQueryApiKey = StringValues.Empty;
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialHeaderApiKey)
            && !context.HttpContext.Request.Query.TryGetValue(ApiKeyQueryName, out potentialQueryApiKey))
            RefuseAccess(context, "API key verplicht.");

        var potentialApiKey = string.Empty;
        if (!string.IsNullOrWhiteSpace(potentialQueryApiKey)) potentialApiKey = potentialQueryApiKey;

        if (!string.IsNullOrWhiteSpace(potentialHeaderApiKey)) potentialApiKey = potentialHeaderApiKey;

        if (string.IsNullOrWhiteSpace(potentialApiKey)) RefuseAccess(context, "API key verplicht.");

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiTokenHeaderName, out var potentialHeaderApiTokens))
        {
            RefuseAccess(context, "Gelieve een geldige API key op te geven");
            return next();
        }

        var potentialHeaderApiToken = potentialHeaderApiTokens.FirstOrDefault();
        if (potentialHeaderApiToken is null)
        {
            RefuseAccess(context, "Ongeldige API key");
            return next();
        }

        var bytes = Convert.FromBase64String(potentialHeaderApiToken);
        var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        var apiToken = JsonConvert.DeserializeObject<ApiToken>(json);

        var wrAccess = apiToken?.Metadata.WrAccess;
        var syncAccess = apiToken?.Metadata.SyncAccess;
        var ticketsAccess = apiToken?.Metadata.TicketsAccess;

        if ((_requiredAccess.Equals("road", StringComparison.InvariantCultureIgnoreCase) && !(wrAccess ?? false))
            || (_requiredAccess.Equals("sync", StringComparison.InvariantCultureIgnoreCase) && !(syncAccess ?? false))
            || (_requiredAccess.Equals("tickets", StringComparison.InvariantCultureIgnoreCase) && !(ticketsAccess ?? false)))
        {
            RefuseAccess(context, "Geen toegang");
            return next();
        }

        return next();
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.Request.Headers.ContainsKey(ApiTokenHeaderName))
        {
            await OnActionExecutionApiTokenAsync(context, next);
            return;
        }

        await OnActionExecutionApiKeyAsync(context, next);
    }

    private static void RefuseAccess(ActionExecutingContext context, string message)
    {
        context.SetContentFormatAcceptType();
        throw new ApiException(message, StatusCodes.Status401Unauthorized);
    }
}