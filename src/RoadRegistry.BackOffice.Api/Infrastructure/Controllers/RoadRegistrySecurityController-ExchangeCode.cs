namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Authentication;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using FluentValidation;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
        var parameters = new ExchangeCodeParameters
        {
            Code = code,
            Verifier = verifier
        };
        await new ExchangeCodeParametersValidator().ValidateAndThrowAsync(parameters, cancellationToken);
        
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
            throw new AuthorizationCodeTokenException(tokenResponse, tokenEndpointAddress);
        }

        var token = new JwtSecurityToken(tokenResponse.IdentityToken);
        var identity = new ClaimsIdentity();
        identity.AddClaims(token.Claims);

        AddRoadRegistryScopes(identity, token.Claims);

        var tokenBuilder = new RoadRegistryTokenBuilder(_openIdConnectOptions);
        var jwtToken = tokenBuilder.BuildJwt(identity);

        return Ok(jwtToken);
    }

    private void AddRoadRegistryScopes(ClaimsIdentity identity, IEnumerable<Claim> claims)
    {
        var scopes = claims
            .Select(RoadRegistryClaim.ReadFrom)
            .Where(claim => claim is not null)
            .SelectMany(claim => RoadRegistryRoles.GetScopes(claim.Role))
            .Distinct()
            .ToArray();
        foreach (var scope in scopes)
        {
            identity.AddClaim(new Claim(AcmIdmClaimTypes.Scope, scope));
        }
    }

    private sealed class ExchangeCodeParameters
    {
        public string Code { get; set; }
        public string Verifier { get; set; }
    }

    private sealed class ExchangeCodeParametersValidator : AbstractValidator<ExchangeCodeParameters>
    {
        public ExchangeCodeParametersValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithProblemCode(ProblemCode.Common.IsRequired);
            RuleFor(x => x.Verifier)
                .NotEmpty()
                .WithProblemCode(ProblemCode.Common.IsRequired);
        }
    }
}
