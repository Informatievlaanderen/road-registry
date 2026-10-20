using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;

namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization
{
    public static class AuthorizationExtensions
    {
        public static AuthorizationOptions AddAcmIdmPolicyVoInfo(this AuthorizationOptions authorizationOptions)
        {
            return authorizationOptions.AddPolicy(AcmIdmConstants.PolicyNames.VoInfo, Scopes.VoInfo);
        }

        public static AuthorizationOptions AddAcmIdmPolicyWegBeheerder(this AuthorizationOptions authorizationOptions)
        {
            return authorizationOptions.AddPolicy(AcmIdmConstants.PolicyNames.WegBeheerder,
                Scopes.DvWrGeschetsteWegBeheer,
                Scopes.DvWrIngemetenWegBeheer);
        }

        // Any one of the given scopes satisfies the policy.
        private static AuthorizationOptions AddPolicy(this AuthorizationOptions options, string policyName, params string[] scopes)
        {
            options.AddPolicy(policyName, policyBuilder => policyBuilder.AddRequirements(new AcmIdmAuthorizationRequirement(scopes)));

            return options;
        }
    }
}
