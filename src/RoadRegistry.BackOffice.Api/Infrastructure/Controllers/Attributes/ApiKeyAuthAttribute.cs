namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        if (!apiToken.Metadata.WrAccess)
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
                nameof(ApiTokenMetadata.WrAccess)
            },
            ConsistentRead = true
        };

        var response = await dbClient.GetItemAsync(request);

        if (!response.IsItemSet)
        {
            throw RefuseAccess(context);
        }

        return new ApiToken(
            response.Item.Keys.First(),
            response.Item.Values.ElementAt(2).S,
            new ApiTokenMetadata(
                response.Item.Values.ElementAt(1).BOOL)
        );
    }

    private static ApiException RefuseAccess(AuthorizationFilterContext context)
    {
        context.SetContentFormatAcceptType();
        return new ApiException("Geen geldige API key.", StatusCodes.Status401Unauthorized);
    }

    internal record ApiToken([JsonProperty("apikey")] string ApiKey, [JsonProperty("clientname")] string ClientName, [JsonProperty("metadata")] ApiTokenMetadata Metadata);

    internal record ApiTokenMetadata([JsonProperty("wraccess")] bool WrAccess, [JsonProperty("syncaccess")] bool SyncAccess = false, [JsonProperty("ticketsaccess")] bool TicketsAccess = false);
}
