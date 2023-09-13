namespace RoadRegistry.BackOffice.Extracts;

public static class ExtractFileNameExtensions
{
    public static string ToCpgFileName(this ExtractFileName fileName)
    {
        return $"{fileName}.cpg";
    }
    public static string ToDbaseFileName(this ExtractFileName fileName)
    {
        return $"{fileName}.dbf";
    }
    public static string ToShapeFileName(this ExtractFileName fileName)
    {
        return $"{fileName}.shp";
    }
    public static string ToShapeIndexFileName(this ExtractFileName fileName)
    {
        return $"{fileName}.shx";
    }
    public static string ToProjectionFileName(this ExtractFileName fileName)
    {
        return $"{fileName}.prj";
    }
}
