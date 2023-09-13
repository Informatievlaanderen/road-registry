namespace RoadRegistry.BackOffice.FeatureCompare;

using Extracts;

public static class FeatureTypeExtensions
{
    public static string GetDbaseFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToDbaseFileName()}";
    }
    public static string GetShapeFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToShapeFileName()}";
    }
    public static string GetShapeIndexFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToShapeIndexFileName()}";
    }
    public static string GetProjectionFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToProjectionFileName()}";
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
