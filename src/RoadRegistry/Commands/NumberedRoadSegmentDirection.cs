namespace RoadRegistry.Commands
{
    public enum NumberedRoadSegmentDirection
    {
        Unknown, //-8	niet gekend	Geen informatie beschikbaar
        Forward, // 1	gelijklopend met de digitalisatiezin	Nummering weg slaat op de richting die de digitalisatiezin van het wegsegment volgt.
        Backward // 2	tegengesteld aan de digitalisatiezin	Nummering weg slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment.
    }
}