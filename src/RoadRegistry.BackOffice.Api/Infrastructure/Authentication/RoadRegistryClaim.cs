namespace RoadRegistry.BackOffice.Api.Infrastructure.Authentication;

using System.Linq;
using System.Security.Claims;

public sealed record RoadRegistryClaim(string Role)
{
    public const string ClaimType = "dv_wegenregister";
    private const string ClaimValuePrefix = "DVWegenregister-";

    public static RoadRegistryClaim ReadFrom(Claim claim)
    {
        if (claim.Type != ClaimType)
        {
            return null;
        }

        var role = claim.Value.Replace(ClaimValuePrefix, string.Empty).Split(':').First();
        return new RoadRegistryClaim(role);
    }
}
