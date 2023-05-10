namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Uploads;

public enum FeatureType
{
    Extract,
    Levering,
    Integration
}

internal abstract class FeatureCompareTranslatorBase<TAttributes> : IZipArchiveEntryFeatureCompareTranslator
    where TAttributes : class
{
    protected record Feature(RecordNumber RecordNumber, TAttributes Attributes);

    protected FeatureCompareTranslatorBase(Encoding encoding)
    {
        Encoding = encoding;
    }

    protected Encoding Encoding { get; }

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
    protected string GetDbfFileName(FeatureType featureType, string fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.dbf";
    }
    protected string GetShpFileName(FeatureType featureType, string fileName)
    {
        return $"{GetFileNamePrefix(featureType)}{fileName}.shp";
    }

    protected abstract List<Feature> ReadFeatures(FeatureType featureType, IReadOnlyCollection<ZipArchiveEntry> entries, string fileName);

    protected (List<Feature>, List<Feature>) ReadExtractAndLeveringFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
    {
        var extractFeatures = ReadFeatures(FeatureType.Extract, entries, fileName);
        var leveringFeatures = ReadFeatures(FeatureType.Levering, entries, fileName);
        return (extractFeatures, leveringFeatures);
    }

    protected (List<Feature>, List<Feature>, List<Feature>) ReadExtractAndLeveringAndIntegrationFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
    {
        var extractFeatures = ReadFeatures(FeatureType.Extract, entries, fileName);
        var leveringFeatures = ReadFeatures(FeatureType.Levering, entries, fileName);
        var integrationFeatures = ReadFeatures(FeatureType.Integration, entries, fileName);
        return (extractFeatures, leveringFeatures, integrationFeatures);
    }
    
    public abstract Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
