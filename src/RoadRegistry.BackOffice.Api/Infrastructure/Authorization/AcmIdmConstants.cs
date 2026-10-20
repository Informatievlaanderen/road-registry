namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization;

public static class AcmIdmConstants
{
    public static class PolicyNames
    {
        public const string VoInfo = "vo-info";

        // Every road segment edit endpoint sits on this one: it takes the 'geschetste weg' scope as well as the
        // 'ingemeten weg' scope, so only a caller holding neither is answered with a 403. Which of the two kinds of
        // road segment they may then actually touch is decided by the domain, per segment.
        public const string WegBeheerder = "wegen-weg-beheerder";
    }
}
