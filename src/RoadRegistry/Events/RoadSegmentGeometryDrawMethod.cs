namespace RoadRegistry.Events
{
    public enum RoadSegmentGeometryDrawMethod
    {
        Outlined, // 1	ingeschetst	Wegsegment waarvan de geometrie ingeschetst werd.
        Measured, // 2	ingemeten	Wegsegment waarvan de geometrie ingemeten werd (bv. overgenomen uit as-built-plan of andere dataset).
        Measured_according_to_GRB_specifications // 3	ingemeten volgens GRB-specificaties	Wegsegment waarvan de geometrie werd ingemeten volgens GRB-specificaties.
    }
}