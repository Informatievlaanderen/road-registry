namespace RoadRegistry.BackOffice.Api.Infrastructure
{
    using System;
    using System.Runtime.Serialization;
    using Exceptions;
    using IdentityModel.Client;

    [Serializable]
    public class AuthorizationCodeTokenException : RoadRegistryException
    {
        public AuthorizationCodeTokenException() { }

        public AuthorizationCodeTokenException(TokenResponse tokenResponse, string tokenEndpointAddress)
            : base(
                $"[Error] {tokenResponse.Error}\n" +
                $"[ErrorDescription] {tokenResponse.ErrorDescription}\n" +
                $"[TokenEndpoint] {tokenEndpointAddress}",
                tokenResponse.Exception) { }
        
        protected AuthorizationCodeTokenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
