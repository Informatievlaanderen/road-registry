namespace RoadRegistry.BackOffice.Api.Extracten;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

// Shared by every endpoint that takes a contour from the caller, whether as WKT or read from a shape file. A contour
// whose geometry does not hold up must not be stored: assembling the extract looks up the road network inside the
// contour with a spatial query, and SQL Server refuses any spatial operation on an invalid instance, so an invalid
// contour is not noticed until the extract itself fails.
//
// The size limit is not shared. It belongs to the contour taken as WKT, which serves the UI and the requests GRB
// sends in itself; a contour read from a shape file can be far larger than that and is accepted on its geometry alone.
public sealed class ExtractContourValidator
{
    private const int SquareKmMaximum = 200; // The UI will limit on 100km2, and GRB as well. This is set higher to ensure the buffered GRB geometry will always be accepted.

    public bool IsValid(string contour)
    {
        try
        {
            var geometry = new WKTReader().Read(contour);

            return IsValidGeometry(geometry)
                   && geometry.Area <= (SquareKmMaximum * 1000 * 1000);
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidGeometry(Geometry? contour)
    {
        return contour is Polygon or MultiPolygon
               && contour.IsValid;
    }
}
