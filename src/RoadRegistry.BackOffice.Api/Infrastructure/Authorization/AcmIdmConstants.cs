namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization;

public static class AcmIdmConstants
{
    public static class PolicyNames
    {
        public const string VoInfo = "vo-info";

        // The road segment edit endpoints share this one: it takes the 'geschetste weg' scope as well as the
        // 'ingemeten weg' scope, so only a caller holding neither is answered with a 403. Which of the two kinds of
        // road segment they may then actually touch is decided by the domain, per segment.
        //
        // The road node and kruising endpoints are not on it: they stay on the 'geschetste weg' scope alone.
        public const string WegBeheerder = "wegen-weg-beheerder";
    }
}
