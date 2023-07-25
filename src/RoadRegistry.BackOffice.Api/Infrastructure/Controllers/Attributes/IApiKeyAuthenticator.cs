namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

internal interface IApiKeyAuthenticator
{
    Task<IIdentity> AuthenticateAsync(string apiKey, CancellationToken cancellationToken);
    Task<IIdentity> AuthenticateAsync(ApiToken apiToken, CancellationToken cancellationToken);
}
