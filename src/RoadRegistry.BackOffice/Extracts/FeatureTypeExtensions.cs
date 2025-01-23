namespace RoadRegistry.BackOffice.Extracts;

public static class FeatureTypeExtensions
{
    public static string ToDbaseFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToDbaseFileName()}";
    }
    public static string ToShapeFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToShapeFileName()}";
    }
    public static string ToShapeIndexFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToShapeIndexFileName()}";
    }
    public static string ToProjectionFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToProjectionFileName()}";
    }
    public static string ToCpgFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName.ToCpgFileName()}";
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
