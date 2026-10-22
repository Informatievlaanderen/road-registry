namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization;

public static class AcmIdmConstants
{
    public static class PolicyNames
    {
        public const string VoInfo = "vo-info";

        // Wijzigen geometrie, the road segment status changes and wijzigen geometriemethode sit on this one: it takes
        // the 'geschetste weg' scope as well as the 'ingemeten weg' scope, so only a caller holding neither is
        // answered with a 403. Which of the two kinds of road segment they may then actually touch is decided by the
        // domain, per segment - except for wijzigen geometriemethode, where the 403 is the whole of it.
        //
        // Every other edit endpoint stays on the 'geschetste weg' scope alone.
        public const string WegBeheerder = "wegen-weg-beheerder";
    }
}
