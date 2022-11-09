namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string ApiKeyHeaderName = "x-api-key";
    private const string ApiKeyQueryName = "apikey";
    private const string ApiTokenHeaderName = "x-api-token";
    private readonly WellKnownAuthRoles _requiredAccess;

    public ApiKeyAuthAttribute(WellKnownAuthRoles requiredAccess)
    {
        _requiredAccess = requiredAccess;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var sp = context.HttpContext.RequestServices;
        var authenticationFeatureToggle = sp.GetRequiredService<UseApiKeyAuthenticationFeatureToggle>();

        if (authenticationFeatureToggle.FeatureEnabled && !context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            var apiToken = context.GetValueFromHeader(ApiTokenHeaderName);
            if (apiToken is not null)
            {
                var bytes = Convert.FromBase64String(apiToken);
                var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                await OnAuthorizationTokenAsync(context, JsonConvert.DeserializeObject<ApiToken>(json));
                return;
            }

            var apiKey = context.GetValueFromHeader(ApiKeyHeaderName) ?? context.GetValueFromQueryString(ApiKeyQueryName);
            if (apiKey is not null)
            {
                await OnAuthorizationKeyAsync(context, apiKey);
                return;
            }

            throw RefuseAccess(context);
        }
    }

    private async Task OnAuthorizationKeyAsync(AuthorizationFilterContext context, string apiKey)
        => await OnAuthorizationTokenAsync(context, async () => await ReadFromDynamoDbAsync(context, apiKey));

    private async Task OnAuthorizationTokenAsync(AuthorizationFilterContext context, ApiToken apiToken)
        => await OnAuthorizationTokenAsync(context, async () => await ReadFromDynamoDbAsync(context, apiToken.ApiKey));

    private async Task OnAuthorizationTokenAsync(AuthorizationFilterContext context, Func<Task<ApiToken>> callback)
    {
        var apiToken = await callback();

        var hasAccess = _requiredAccess switch
        {
            WellKnownAuthRoles.Road => apiToken.Metadata.WrAccess,
            WellKnownAuthRoles.Sync => apiToken.Metadata.SyncAccess,
            WellKnownAuthRoles.Tickets => apiToken.Metadata.TicketsAccess,
            _ => false
        };

        if (apiToken.Revoked || !hasAccess)
        {
            throw RefuseAccess(context);
        }
    }

    private async Task<ApiToken> ReadFromDynamoDbAsync(AuthorizationFilterContext context, string apiKey)
    {
        var dbClient = context.HttpContext.RequestServices.GetRequiredService<AmazonDynamoDBClient>();

        var request = new GetItemRequest
        {
            TableName = "basisregisters-api-gate-keys",
            Key = new Dictionary<string, AttributeValue> { { nameof(ApiToken.ApiKey), new AttributeValue(apiKey) } },
            AttributesToGet = new List<string>
            {
                nameof(ApiToken.ApiKey),
                nameof(ApiToken.ClientName),
                nameof(ApiToken.Revoked),
                nameof(ApiTokenMetadata.WrAccess),
                nameof(ApiTokenMetadata.SyncAccess),
                "Tickets"
            },
            ConsistentRead = true
        };

        var response = await dbClient.GetItemAsync(request);

        if (!response.IsItemSet)
        {
            throw RefuseAccess(context);
        }

        response.Item.TryGetValue(nameof(ApiToken.ClientName), out var clientName);
        response.Item.TryGetValue(nameof(ApiToken.Revoked), out var revoked);
        response.Item.TryGetValue(nameof(ApiTokenMetadata.WrAccess), out var wrAccess);
        response.Item.TryGetValue(nameof(ApiTokenMetadata.SyncAccess), out var syncAccess);
        response.Item.TryGetValue("Tickets", out var ticketAccess);

        return new ApiToken(
            apiKey,
            clientName?.S ?? "",
            new ApiTokenMetadata(
                wrAccess?.BOOL ?? false,
                syncAccess?.BOOL ?? false,
                ticketAccess?.BOOL ?? false)
        )
        {
            Revoked = revoked?.BOOL ?? false
        };
    }

    private static ApiException RefuseAccess(AuthorizationFilterContext context)
    {
        context.SetContentFormatAcceptType();
        return new ApiException("Geen geldige API key.", StatusCodes.Status401Unauthorized);
    }

    internal record ApiToken([JsonProperty("apikey")] string ApiKey, [JsonProperty("clientname")] string ClientName, [JsonProperty("metadata")] ApiTokenMetadata Metadata)
    {
        [JsonIgnore]
        public bool Revoked { get; set; }
    };

    internal record ApiTokenMetadata([JsonProperty("wraccess")] bool WrAccess, [JsonProperty("syncaccess")] bool SyncAccess = false, [JsonProperty("ticketsaccess")] bool TicketsAccess = false);
}
