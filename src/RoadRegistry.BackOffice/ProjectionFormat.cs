namespace RoadRegistry.BackOffice;

using System;
using System.IO;
using DotSpatial.Projections;

public class ProjectionFormat
{
    public static readonly ProjectionFormat BelgeLambert1972 = new(@"PROJCS[""Belge_Lambert_1972"",GEOGCS[""GCS_Belge_1972"",DATUM[""D_Belge_1972"",SPHEROID[""International_1924"",6378388.0,297.0]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]],PROJECTION[""Lambert_Conformal_Conic""],PARAMETER[""False_Easting"",150000.01256],PARAMETER[""False_Northing"",5400088.4378],PARAMETER[""Central_Meridian"",4.367486666666666],PARAMETER[""Standard_Parallel_1"",49.8333339],PARAMETER[""Standard_Parallel_2"",51.16666723333333],PARAMETER[""Latitude_Of_Origin"",90.0],UNIT[""Meter"",1.0]]");

    public ProjectionFormat(string content)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    public string Content { get; }

    public bool IsBelgeLambert1972()
    {
        try
        {
            var projectionInfo = ProjectionInfo.FromEsriString(Content);
            if (!projectionInfo.IsValid)
            {
                return false;
            }

            return string.Equals(projectionInfo.Name, "Belge_Lambert_1972", StringComparison.OrdinalIgnoreCase);
        }
        catch (InvalidEsriFormatException)
        {
            return false;
        }
    }

    public static ProjectionFormat Read(StreamReader reader)
    {
        return new ProjectionFormat(reader.ReadToEnd());
    }
}
