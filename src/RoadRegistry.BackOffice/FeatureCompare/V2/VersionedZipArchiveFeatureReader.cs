namespace RoadRegistry.BackOffice.FeatureCompare.V2;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;

public class VersionedZipArchiveFeatureReader<TFeature> : IZipArchiveFeatureReader<TFeature>
    where TFeature : class
{
    private readonly IZipArchiveFeatureReader<TFeature>[] _versionedReaders;

    public VersionedZipArchiveFeatureReader(params IZipArchiveFeatureReader<TFeature>[] readers)
    {
        if (!readers.ThrowIfNull().Any())
        {
            throw new ArgumentException("At least 1 reader is required", nameof(readers));
        }

        _versionedReaders = readers;
    }

    public virtual (List<TFeature>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(featureType);
        ArgumentNullException.ThrowIfNull(context);

        var readerProblems = new List<ZipArchiveProblems>();

        foreach (var reader in _versionedReaders)
        {
            var (features, featuresProblems) = reader.Read(archive, featureType, context);

            var requiredFileMissing = featuresProblems.Any(x => x.Reason == nameof(ZipArchiveProblems.RequiredFileMissing));
            if (requiredFileMissing)
            {
                return (features, featuresProblems);
            }

            var hasDbaseHeaderFormatError = featuresProblems.Any(x => x.Reason == nameof(DbaseFileProblems.HasDbaseHeaderFormatError));
            if (hasDbaseHeaderFormatError)
            {
                return (features, featuresProblems);
            }

            var hasDbaseSchemaMismatch = featuresProblems.Any(x => x.Reason == nameof(DbaseFileProblems.HasDbaseSchemaMismatch));
            if (!hasDbaseSchemaMismatch)
            {
                return (features, featuresProblems);
            }

            readerProblems.Add(featuresProblems);
        }

        var problems = readerProblems.Aggregate(ZipArchiveProblems.None, (x1, x2) => x1 + x2);

        return ([], problems);
    }
}
