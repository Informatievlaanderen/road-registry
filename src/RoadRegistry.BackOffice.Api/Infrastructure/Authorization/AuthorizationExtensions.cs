using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.AcmIdm.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;

namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization
{
    public static class AuthorizationExtensions
    {
        public static AuthorizationOptions AddAcmIdmPolicyVoInfo(this AuthorizationOptions authorizationOptions)
        {
            return authorizationOptions.AddPolicy(AcmIdmConstants.PolicyNames.VoInfo, Scopes.VoInfo);
        }

        private static AuthorizationOptions AddPolicy(this AuthorizationOptions options, string policyName, string scope)
        {
            options.AddPolicy(policyName, policyBuilder => policyBuilder.AddRequirements(new AcmIdmAuthorizationRequirement(new []{ scope })));

            return options;
        }
    }
}
