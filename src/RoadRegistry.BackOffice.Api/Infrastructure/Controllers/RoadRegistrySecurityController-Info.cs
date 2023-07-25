namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Configuration;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class RoadRegistrySecurityController
{
    private const string InfoRoute = "info";

    [HttpGet(InfoRoute, Name = nameof(Info))]
    [SwaggerOperation(OperationId = nameof(Info), Description = "")]
    public IActionResult Info()
    {
        return Ok(new OidcClientConfiguration(_openIdConnectOptions));
    }
}

public class OidcClientConfiguration
{
    public OidcClientConfiguration(OpenIdConnectOptions configuration)
    {
        Authority = configuration.Authority;
        Issuer = configuration.AuthorizationIssuer;
        AuthorizationEndpoint = configuration.AuthorizationEndpoint;
        AuthorizationRedirectUri = configuration.AuthorizationRedirectUri;
        UserInfoEndPoint = configuration.UserInfoEndPoint;
        EndSessionEndPoint = configuration.EndSessionEndPoint;
        JwksUri = configuration.JwksUri;
        ClientId = configuration.ClientId;
        PostLogoutRedirectUri = configuration.PostLogoutRedirectUri;
    }

    public string Authority { get; }
    public string Issuer { get; }
    public string AuthorizationEndpoint { get; }
    public string UserInfoEndPoint { get; }
    public string EndSessionEndPoint { get; }
    public string JwksUri { get; }
    public string ClientId { get; }
    public string AuthorizationRedirectUri { get; }
    public string PostLogoutRedirectUri { get; }
}
