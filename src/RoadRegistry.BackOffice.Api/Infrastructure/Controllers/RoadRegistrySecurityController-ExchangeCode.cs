namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Authorization;
using Extensions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySecurityController
{
    private const string ExchangeCodeRoute = "exchange";

    [HttpGet(ExchangeCodeRoute, Name = nameof(ExchangeCode))]
    [SwaggerOperation(OperationId = nameof(ExchangeCode), Description = "")]
    public async Task<IActionResult> ExchangeCode(
        [FromServices] IHttpClientFactory httpClientFactory,
        string code,
        string verifier,
        string? redirectUri,
        CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var tokenEndpointAddress =
            $"{_openIdConnectOptions.Authority}{_openIdConnectOptions.TokenEndPoint}";

        var tokenResponse = await httpClient.RequestAuthorizationCodeTokenAsync(
            new AuthorizationCodeTokenRequest
            {
                ClientId = _openIdConnectOptions.ClientId,
                ClientSecret = _openIdConnectOptions.ClientSecret,
                RedirectUri = redirectUri ?? _openIdConnectOptions.AuthorizationRedirectUri,
                Address = tokenEndpointAddress,
                Code = code,
                CodeVerifier = verifier
            },
            cancellationToken);

        if (tokenResponse.IsError)
        {
            _logger.LogError("[Error] {Error}\nErrorDescription] {ErrorDescription}\nTokenEndpoint] {TokenEndpointAddress}", tokenResponse.Error, tokenResponse.ErrorDescription, tokenEndpointAddress);
            throw new Exception(
                $"[Error] {tokenResponse.Error}\n" +
                $"[ErrorDescription] {tokenResponse.ErrorDescription}\n" +
                $"[TokenEndpoint] {tokenEndpointAddress}",
                tokenResponse.Exception);
        }

        var token = new JwtSecurityToken(tokenResponse.IdentityToken);
        var identity = new ClaimsIdentity();
        identity.AddClaims(token.Claims);

        var tokenBuilder = new RoadRegistryTokenBuilder(_openIdConnectOptions);
        var jwtToken = tokenBuilder.BuildJwt(identity);
        
        return Ok(jwtToken);
    }
}
