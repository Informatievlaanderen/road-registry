using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.AcmIdm.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;

namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization
{
    public static class AuthorizationExtensions
    {
        public static AuthorizationOptions AddAcmIdmPolicyAuthenticated(this AuthorizationOptions authorizationOptions)
        {
            return authorizationOptions.AddPolicy(AcmIdmConstants.PolicyNames.Authenticated, new[]
            {
                Scopes.VoInfo,
                Scopes.DvWrAttribuutWaardenBeheer,
                Scopes.DvWrGeschetsteWegBeheer,
                Scopes.DvWrIngemetenWegBeheer,
                Scopes.DvWrUitzonderingenBeheer,
                AcmIdmConstants.Scopes.DvWegenregister
            });
        }
        public static AuthorizationOptions AddAcmIdmPolicyDvWegenregister(this AuthorizationOptions authorizationOptions)
        {
            return authorizationOptions.AddPolicy(AcmIdmConstants.PolicyNames.Wegenregister, new[]{ AcmIdmConstants.Scopes.DvWegenregister });
        }

        private static AuthorizationOptions AddPolicy(this AuthorizationOptions options, string policyName, string[] scopes)
        {
            options.AddPolicy(policyName, policyBuilder => policyBuilder.AddRequirements(new AcmIdmAuthorizationRequirement(scopes)));

            return options;
        }
    }
}
