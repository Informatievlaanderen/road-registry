namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

public static class Feature
{
    public static Feature<TAttributes> New<TAttributes>(RecordNumber recordNumber, TAttributes attributes)
        where TAttributes : class
    {
        return new Feature<TAttributes>(recordNumber, attributes);
    }
}

public record Feature<TAttributes>(RecordNumber RecordNumber, TAttributes Attributes) where TAttributes : class;

internal abstract class FeatureCompareTranslatorBase<TAttributes> : IZipArchiveEntryFeatureCompareTranslator
    where TAttributes : class
{
    protected FeatureCompareTranslatorBase(Encoding encoding)
    {
        Encoding = encoding;
    }

    protected Encoding Encoding { get; }
    
    protected abstract (List<Feature<TAttributes>>, ZipArchiveProblems) ReadFeatures(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context);

    protected (List<Feature<TAttributes>>, List<Feature<TAttributes>>, ZipArchiveProblems) ReadExtractAndChangeFeatures(ZipArchive archive, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var (extractFeatures, extractFeaturesProblems) = ReadFeatures(archive, FeatureType.Extract, fileName, context);
        var (changeFeatures, changeFeaturesProblems) = ReadFeatures(archive, FeatureType.Change, fileName, context);
        return (extractFeatures, changeFeatures, extractFeaturesProblems + changeFeaturesProblems);
    }

    protected (List<Feature<TAttributes>>, List<Feature<TAttributes>>, List<Feature<TAttributes>>, ZipArchiveProblems) ReadExtractAndLeveringAndIntegrationFeatures(ZipArchive archive, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var (extractFeatures, extractFeaturesProblems) = ReadFeatures(archive, FeatureType.Extract, fileName, context);
        var (changeFeatures, changeFeaturesProblems) = ReadFeatures(archive, FeatureType.Change, fileName, context);
        var (integrationFeatures, integrationFeaturesProblems) = ReadFeatures(archive, FeatureType.Integration, fileName, context);

        var problems = extractFeaturesProblems + changeFeaturesProblems + integrationFeaturesProblems;

        return (extractFeatures, changeFeatures, integrationFeatures, problems);
    }
    
    public abstract Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
