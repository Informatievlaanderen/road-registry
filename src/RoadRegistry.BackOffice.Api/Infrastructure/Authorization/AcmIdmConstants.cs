namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization;

public static class AcmIdmConstants
{
    public static class PolicyNames
    {
        public const string Authenticated = "vo-authenticated";
        public const string Wegenregister = "wegen-wegenregister";
    }

    public static class Scopes
    {
        public const string DvWegenregister = "dv_wegenregister";
    }
}
