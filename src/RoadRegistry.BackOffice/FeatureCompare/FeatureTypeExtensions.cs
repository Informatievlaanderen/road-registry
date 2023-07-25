namespace RoadRegistry.BackOffice.FeatureCompare;

using Extracts;

public static class FeatureTypeExtensions
{
    public static string GetDbaseFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.dbf";
    }
    public static string GetShapeFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.shp";
    }
    public static string GetProjectionFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.prj";
    }

    private static string GetFileNamePrefix(FeatureType featureType)
    {
        switch (featureType)
        {
            case FeatureType.Extract:
                return "e";
            case FeatureType.Integration:
                return "i";
        }

        return string.Empty;
    }
}
