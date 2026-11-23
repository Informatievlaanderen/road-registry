namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization;

public static class AcmIdmConstants
{
    public static class PolicyNames
    {
        public const string VoInfo = "vo-info";

        // Wijzigen geometrie and the road segment status changes sit on this one: it takes the 'geschetste weg' scope
        // as well as the 'ingemeten weg' scope, so only a caller holding neither is answered with a 403. Which of the
        // two kinds of road segment they may then actually touch is decided by the domain, per segment.
        //
        // Wijzigen geometriemethode is not on this policy: only the central road manager may change it, so it takes
        // the 'ingemeten weg' scope on its own. Every other edit endpoint stays on the 'geschetste weg' scope alone.
        public const string WegBeheerder = "wegen-weg-beheerder";
    }
}
