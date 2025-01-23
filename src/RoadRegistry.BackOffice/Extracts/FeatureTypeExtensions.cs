namespace RoadRegistry.BackOffice.Extracts;

public static class FeatureTypeExtensions
{
    public static string ToDbaseFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.dbf";
    }
    public static string ToShapeFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.shp";
    }
    public static string ToShapeIndexFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.shx";
    }
    public static string ToProjectionFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.prj";
    }
    public static string ToCpgFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.cpg";
    }

    private static string GetFileNamePrefix(FeatureType featureType)
    {
        return featureType switch
        {
            FeatureType.Extract => "e",
            FeatureType.Integration => "i",
            _ => string.Empty
        };
    }
}
