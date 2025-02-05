namespace RoadRegistry.BackOffice.Extracts;

public static class ExtractFileNameExtensions
{
    public static string ToCpgFileName(this ExtractFileName fileName, FeatureType featureType = FeatureType.Change)
    {
        return featureType.ToCpgFileName(fileName);
    }
    public static string ToDbaseFileName(this ExtractFileName fileName, FeatureType featureType = FeatureType.Change)
    {
        return featureType.ToDbaseFileName(fileName);
    }
    public static string ToShapeFileName(this ExtractFileName fileName, FeatureType featureType = FeatureType.Change)
    {
        return featureType.ToShapeFileName(fileName);
    }
    public static string ToShapeIndexFileName(this ExtractFileName fileName, FeatureType featureType = FeatureType.Change)
    {
        return featureType.ToShapeIndexFileName(fileName);
    }
    public static string ToProjectionFileName(this ExtractFileName fileName, FeatureType featureType = FeatureType.Change)
    {
        return featureType.ToProjectionFileName(fileName);
    }
}
