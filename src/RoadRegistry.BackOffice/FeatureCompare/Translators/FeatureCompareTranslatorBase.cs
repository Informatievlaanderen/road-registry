namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Be.Vlaanderen.Basisregisters.Shaperon;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

public record Feature<TAttributes>(RecordNumber RecordNumber, TAttributes Attributes) where TAttributes : class;

internal abstract class FeatureCompareTranslatorBase<TAttributes> : IZipArchiveEntryFeatureCompareTranslator
    where TAttributes : class
{
    protected FeatureCompareTranslatorBase(Encoding encoding)
    {
        Encoding = encoding;
    }

    protected Encoding Encoding { get; }
    
    protected abstract List<Feature<TAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, string fileName);

    protected (List<Feature<TAttributes>>, List<Feature<TAttributes>>) ReadExtractAndLeveringFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
    {
        var extractFeatures = ReadFeatures(entries, FeatureType.Extract, fileName);
        var leveringFeatures = ReadFeatures(entries, FeatureType.Levering, fileName);
        return (extractFeatures, leveringFeatures);
    }

    protected (List<Feature<TAttributes>>, List<Feature<TAttributes>>, List<Feature<TAttributes>>) ReadExtractAndLeveringAndIntegrationFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
    {
        var extractFeatures = ReadFeatures(entries, FeatureType.Extract, fileName);
        var leveringFeatures = ReadFeatures(entries, FeatureType.Levering, fileName);
        var integrationFeatures = ReadFeatures(entries, FeatureType.Integration, fileName);
        return (extractFeatures, leveringFeatures, integrationFeatures);
    }
    
    public abstract Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
