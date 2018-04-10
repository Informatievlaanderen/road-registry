namespace RoadRegistry.Events
{
    public enum LaneDirection
    {
        Unknown, // -8	niet gekend	Geen informatie beschikbaar
        Forward, // 1	gelijklopend met de digitalisatiezin	Aantal rijstroken slaat op de richting die de digitalisatiezin van het wegsegment volgt.
        Backward, // 2	tegengesteld aan de digitalisatiezin	Aantal rijstroken slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment.
        Independent // 3	onafhankelijk van de digitalisatiezin	Aantal rijstroken slaat op het totaal in beide richtingen, onafhankelijk van de digitalisatiezin van het wegsegment.
    }
}