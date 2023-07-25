namespace RoadRegistry.BackOffice.Api.Infrastructure;

using System.Security.Claims;

public interface IRoadRegistryTokenBuilder
{
    string BuildJwt(ClaimsIdentity identity);
}