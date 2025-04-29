namespace RoadRegistry.BackOffice.Api.Infrastructure;

using Exceptions;
using IdentityModel.Client;

public class AuthorizationCodeTokenException : RoadRegistryException
{
    public AuthorizationCodeTokenException() { }

    public AuthorizationCodeTokenException(TokenResponse tokenResponse, string tokenEndpointAddress)
        : base(
            $"[Error] {tokenResponse.Error}\n" +
            $"[ErrorDescription] {tokenResponse.ErrorDescription}\n" +
            $"[TokenEndpoint] {tokenEndpointAddress}",
            tokenResponse.Exception) { }
}
