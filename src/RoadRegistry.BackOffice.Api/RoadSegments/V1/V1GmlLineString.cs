namespace RoadRegistry.BackOffice.Api.RoadSegments.V1;

using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GML2;
using RoadRegistry.Extensions;

// The v1 endpoints keep accepting what they always accepted: GMLReader stops at the first geometry and ignores the rest
// of the document. Everything else uses the stricter GeometryTranslator.ParseGmlLineString.
public static class V1GmlLineString
{
    public static bool IsValid(string gml)
    {
        try
        {
            Parse(gml);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static MultiLineString Parse(string gml)
    {
        var geometry = new GMLReader(WellKnownGeometryFactories.WithoutSrid).Read(gml.ThrowIfNull());
        if (geometry == null)
        {
            throw new InvalidOperationException("The GML is invalid");
        }

        if (!(geometry is LineString))
        {
            throw new InvalidOperationException("The GML is not a LineString");
        }

        return geometry
            .ToMultiLineString(WellKnownGeometryFactories.WithoutSrid)
            .WithMeasureOrdinates();
    }
}
