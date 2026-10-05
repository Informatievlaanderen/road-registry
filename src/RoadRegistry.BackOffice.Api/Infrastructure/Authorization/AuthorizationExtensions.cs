using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization
{
    public static class AuthorizationExtensions
    {
        /// <summary>
        ///     Takes the place of AddAcmIdmAuthorizationHandlers() of the package. See
        ///     <see cref="RoadRegistryAcmIdmAuthorizationHandler" /> for why the package handler is not registered.
        /// </summary>
        public static IServiceCollection AddRoadRegistryAcmIdmAuthorizationHandlers(this IServiceCollection services)
        {
            return services.AddSingleton<IAuthorizationHandler, RoadRegistryAcmIdmAuthorizationHandler>();
        }

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
