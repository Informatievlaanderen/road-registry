namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Be.Vlaanderen.Basisregisters.Shaperon;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
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
    
    protected abstract List<Feature<TAttributes>> ReadFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName);

    protected (List<Feature<TAttributes>>, List<Feature<TAttributes>>) ReadExtractAndChangeFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, ExtractFileName fileName)
    {
        var extractFeatures = ReadFeatures(entries, FeatureType.Extract, fileName);
        var changeFeatures = ReadFeatures(entries, FeatureType.Change, fileName);
        return (extractFeatures, changeFeatures);
    }

    protected (List<Feature<TAttributes>>, List<Feature<TAttributes>>, List<Feature<TAttributes>>) ReadExtractAndLeveringAndIntegrationFeatures(IReadOnlyCollection<ZipArchiveEntry> entries, ExtractFileName fileName)
    {
        var extractFeatures = ReadFeatures(entries, FeatureType.Extract, fileName);
        var changeFeatures = ReadFeatures(entries, FeatureType.Change, fileName);
        var integrationFeatures = ReadFeatures(entries, FeatureType.Integration, fileName);
        return (extractFeatures, changeFeatures, integrationFeatures);
    }
    
    public abstract Task<TranslatedChanges> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
