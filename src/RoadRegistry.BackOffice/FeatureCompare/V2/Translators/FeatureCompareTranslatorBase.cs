namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
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

public abstract class FeatureCompareTranslatorBase<TAttributes> : IZipArchiveEntryFeatureCompareTranslator
    where TAttributes : class
{
    private readonly IZipArchiveFeatureReader<Feature<TAttributes>> _featureReader;

    protected FeatureCompareTranslatorBase(IZipArchiveFeatureReader<Feature<TAttributes>> featureReader)
    {
        _featureReader = featureReader;
    }

    protected (List<Feature<TAttributes>>, ZipArchiveProblems) ReadFeatures(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        return _featureReader.Read(archive, featureType, context);
    }

    protected (List<Feature<TAttributes>>, List<Feature<TAttributes>>, ZipArchiveProblems) ReadExtractAndChangeFeatures(ZipArchive archive, ZipArchiveFeatureReaderContext context)
    {
        var (extractFeatures, extractFeaturesProblems) = ReadFeatures(archive, FeatureType.Extract, context);
        var (changeFeatures, changeFeaturesProblems) = ReadFeatures(archive, FeatureType.Change, context);

        return (extractFeatures, changeFeatures, extractFeaturesProblems + changeFeaturesProblems);
    }

    protected (List<Feature<TAttributes>>, List<Feature<TAttributes>>, List<Feature<TAttributes>>, ZipArchiveProblems) ReadExtractAndChangeAndIntegrationFeatures(ZipArchive archive, ZipArchiveFeatureReaderContext context)
    {
        var (extractFeatures, extractFeaturesProblems) = ReadFeatures(archive, FeatureType.Extract, context);
        var (changeFeatures, changeFeaturesProblems) = ReadFeatures(archive, FeatureType.Change, context);
        var (integrationFeatures, integrationFeaturesProblems) = ReadFeatures(archive, FeatureType.Integration, context);

        var problems = extractFeaturesProblems + changeFeaturesProblems + integrationFeaturesProblems;

        return (extractFeatures, changeFeatures, integrationFeatures, problems);
    }

    public abstract Task<(TranslatedChanges, ZipArchiveProblems)> TranslateAsync(ZipArchiveEntryFeatureCompareTranslateContext context, TranslatedChanges changes, CancellationToken cancellationToken);
}
