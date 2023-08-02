namespace RoadRegistry.BackOffice.Api.Infrastructure.Authentication;

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Controllers.Attributes;
using FeatureToggles;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyAuthenticator _apiKeyAuthenticator;
    private readonly UseApiKeyAuthenticationFeatureToggle _useApiKeyAuthenticationFeatureToggle;
    internal const string ApiKeyQueryName = "apikey";
    internal const string ApiKeyHeaderName = "x-api-key";
    internal const string ApiTokenHeaderName = "x-api-token";

    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IApiKeyAuthenticator apiKeyAuthenticator,
        UseApiKeyAuthenticationFeatureToggle useApiKeyAuthenticationFeatureToggle)
        : base(options, logger, encoder, clock)
    {
        _apiKeyAuthenticator = apiKeyAuthenticator;
        _useApiKeyAuthenticationFeatureToggle = useApiKeyAuthenticationFeatureToggle;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_useApiKeyAuthenticationFeatureToggle.FeatureEnabled)
        {
            return AuthenticateResult.NoResult();
        }

        if (Context.User.Identity?.IsAuthenticated == true)
        {
            return AuthenticateResult.NoResult();
        }

        IIdentity identity = null;
        
        if (Context.Request.Headers.TryGetValue(ApiTokenHeaderName, out var apiTokens))
        {
            var apiToken = apiTokens.FirstOrDefault();
            identity = await _apiKeyAuthenticator.AuthenticateAsync(ApiToken.FromBase64String(apiToken), Context.RequestAborted);
        }
        else if (Context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiHeaderKeys))
        {
            var apiKey = apiHeaderKeys.FirstOrDefault();
            identity = await _apiKeyAuthenticator.AuthenticateAsync(apiKey, Context.RequestAborted);
        }
        else if (Context.Request.Query.TryGetValue(ApiKeyQueryName, out var apiQueryKeys))
        {
            var apiKey = apiQueryKeys.FirstOrDefault();
            identity = await _apiKeyAuthenticator.AuthenticateAsync(apiKey, Context.RequestAborted);
        }

        if (identity is not null && identity.IsAuthenticated)
        {
            return AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationSchemes.ApiKey)
            );
        }

        return AuthenticateResult.NoResult();
    }
}
