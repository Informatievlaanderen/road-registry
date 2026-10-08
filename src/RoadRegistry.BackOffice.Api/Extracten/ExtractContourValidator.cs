namespace RoadRegistry.BackOffice.Api.Extracten;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

// Shared by every endpoint that takes a contour from the caller, whether as WKT or read from a shapefile. A contour
// that does not pass here must not be stored: assembling the extract looks up the road network inside the contour with
// a spatial query, and SQL Server refuses any spatial operation on an invalid instance, so an invalid contour is not
// noticed until the extract itself fails.
public sealed class ExtractContourValidator
{
    private const int SquareKmMaximum = 200; // The UI will limit on 100km2, and GRB as well. This is set higher to ensure the buffered GRB geometry will always be accepted.

    public bool IsValid(string contour)
    {
        try
        {
            return IsValid(new WKTReader().Read(contour));
        }
        catch
        {
            return false;
        }
    }

    public bool IsValid(Geometry? contour)
    {
        return contour is Polygon or MultiPolygon
               && contour.IsValid
               && contour.Area <= (SquareKmMaximum * 1000 * 1000);
    }
}
