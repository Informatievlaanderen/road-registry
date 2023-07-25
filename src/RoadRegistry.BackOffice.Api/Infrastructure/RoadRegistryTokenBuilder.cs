namespace RoadRegistry.BackOffice.Api.Infrastructure;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Configuration;
using Microsoft.IdentityModel.Tokens;

public class RoadRegistryTokenBuilder : IRoadRegistryTokenBuilder
{
    private readonly OpenIdConnectOptions _openIdConnectOptions;

    public RoadRegistryTokenBuilder(OpenIdConnectOptions openIdConnectOptions)
    {
        _openIdConnectOptions = openIdConnectOptions;
    }

    public string BuildJwt(ClaimsIdentity identity)
    {
        var plainTextSecurityKey = _openIdConnectOptions.JwtSharedSigningKey;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(plainTextSecurityKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = _openIdConnectOptions.JwtAudience,
            Issuer = _openIdConnectOptions.JwtIssuer,
            Subject = identity,
            SigningCredentials = signingCredentials,
            IssuedAt = DateTime.UtcNow,
            Expires = DateTimeOffset.Now.AddMinutes(_openIdConnectOptions.JwtExpiresInMinutes).UtcDateTime,
            NotBefore = DateTime.UtcNow
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
        var signedAndEncodedToken = tokenHandler.WriteToken(plainToken);

        return signedAndEncodedToken;
    }
}
