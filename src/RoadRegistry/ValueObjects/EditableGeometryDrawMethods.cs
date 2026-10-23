namespace RoadRegistry.ValueObjects;

// Which road segments the caller may modify, expressed as the geometry draw methods they hold: the 'geschetste weg'
// scope carries the ingeschetste road segments, the 'ingemeten weg' scope the ingemeten ones. The API only reads the
// scopes off the token - a caller holding neither never gets this far, it is answered with a 403 - and the entitlement
// travels along with the request, because which road segments a change actually touches is known to the domain alone.
public sealed record EditableGeometryDrawMethods(bool Ingeschetst, bool Ingemeten)
{
    public bool Allows(RoadSegmentGeometryDrawMethodV2? geometryDrawMethod)
    {
        if (geometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        {
            return Ingeschetst;
        }

        if (geometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten)
        {
            return Ingemeten;
        }

        // A road segment that has not completed its inwinning carries no V2 attributes yet, so it has no draw method
        // to judge. That it cannot be changed at all is reported elsewhere, as a problem of its own.
        return true;
    }
}
