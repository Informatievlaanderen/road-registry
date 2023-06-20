namespace RoadRegistry.BackOffice.FeatureCompare;

using System.Collections.Generic;
using System.IO.Compression;
using Extracts;

public interface IFeatureReader<TFeature>
{
    List<TFeature> Read(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName);
}

public static class FeatureTypeExtensions
{
    public static string GetDbfFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.dbf";
    }
    public static string GetShpFileName(this FeatureType featureType, ExtractFileName fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.shp";
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
