namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string ApiKeyHeaderName = "x-api-key";
    private const string ApiKeyQueryName = "apikey";
    private const string ApiTokenHeaderName = "x-api-token";
    private readonly string _requiredAccess;

    public ApiKeyAuthAttribute(string requiredAccess)
    {
        _requiredAccess = requiredAccess;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var sp = context.HttpContext.RequestServices;
        var authenticationFeatureToggle = sp.GetRequiredService<UseApiKeyAuthenticationFeatureToggle>();

        if (!authenticationFeatureToggle.FeatureEnabled)
        {
            return;
        }

        if (context.HttpContext.Request.Headers.ContainsKey(ApiTokenHeaderName))
        {
            await OnAuthorizationApiTokenAsync(context);
            return;
        }

        await OnAuthorizationApiKeyAsync(context);
    }

    private async Task<bool> CheckIfApiKeyHasAccess(AuthorizationFilterContext context, string apiKey)
    {
        var validApiKeys = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IConfiguration>()
            .GetSection($"ApiKeys:{_requiredAccess}")
            .GetChildren()
            .Select(c => c.Value)
            .ToArray();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            await RefuseAccess(context, "API key verplicht.");
            return false;
        }
        else if (!validApiKeys.Contains(apiKey))
        {
            await RefuseAccess(context, "Ongeldige API key.");
            return false;
        }
        else
        {
            return true;
        }
    }

    public async Task OnAuthorizationApiKeyAsync(AuthorizationFilterContext context)
    {
        var potentialQueryApiKey = StringValues.Empty;
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialHeaderApiKey)
            && !context.HttpContext.Request.Query.TryGetValue(ApiKeyQueryName, out potentialQueryApiKey))
        {
            await RefuseAccess(context, "API key verplicht.");
        }
        else
        {

            var potentialApiKey = string.Empty;
            if (!string.IsNullOrWhiteSpace(potentialQueryApiKey)) potentialApiKey = potentialQueryApiKey;
            if (!string.IsNullOrWhiteSpace(potentialHeaderApiKey)) potentialApiKey = potentialHeaderApiKey;

            await CheckIfApiKeyHasAccess(context, potentialApiKey);
        }
    }

    public async Task OnAuthorizationApiTokenAsync(AuthorizationFilterContext context)
    {
        // We get the x-api-key header or query param string
        // Check if the user used this and thus is not anonymous GAWR-2968
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiTokenHeaderName, out var potentialHeaderApiTokens))
        {
            await RefuseAccess(context, "Gelieve een geldige API key op te geven");
            return;
        }

        var potentialHeaderApiToken = potentialHeaderApiTokens.FirstOrDefault();
        if (potentialHeaderApiToken is null)
        {
            await RefuseAccess(context, "Ongeldige API key");
            return;
        }

        var bytes = Convert.FromBase64String(potentialHeaderApiToken);
        var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        var apiToken = JsonConvert.DeserializeObject<ApiToken>(json);

        var hasAccess = await CheckIfApiKeyHasAccess(context, apiToken?.ApiKey);

        if (hasAccess)
        {
            var wrAccess = apiToken?.Metadata.WrAccess;
            var syncAccess = apiToken?.Metadata.SyncAccess;
            var ticketsAccess = apiToken?.Metadata.TicketsAccess;

            if ((_requiredAccess.Equals("road", StringComparison.InvariantCultureIgnoreCase) && !(wrAccess ?? false))
                || (_requiredAccess.Equals("sync", StringComparison.InvariantCultureIgnoreCase) && !(syncAccess ?? false))
                || (_requiredAccess.Equals("tickets", StringComparison.InvariantCultureIgnoreCase) && !(ticketsAccess ?? false)))
            {
                await RefuseAccess(context, "Geen toegang");
                return;
            }
        }
    }

    private static Task RefuseAccess(AuthorizationFilterContext context, string message)
    {
        context.SetContentFormatAcceptType();
        context.Result = new UnauthorizedObjectResult(message);

        return Task.CompletedTask;
    }

    internal record ApiToken([JsonProperty("clientname")] string ClientName, [JsonProperty("apikey")] string ApiKey, [JsonProperty("metadata")] ApiTokenMetadata Metadata);

    internal record ApiTokenMetadata([JsonProperty("wraccess")] bool WrAccess, [JsonProperty("syncaccess")] bool SyncAccess, [JsonProperty("ticketsaccess")] bool TicketsAccess = false);
}
