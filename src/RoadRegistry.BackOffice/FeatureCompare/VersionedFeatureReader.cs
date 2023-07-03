namespace RoadRegistry.BackOffice.FeatureCompare;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using Uploads;

public class VersionedFeatureReader<TFeature> : IFeatureReader<TFeature>
    where TFeature: class
{
    private readonly IFeatureReader<TFeature>[] _versionedReaders;

    public VersionedFeatureReader(params IFeatureReader<TFeature>[] readers)
    {
        if (!readers.ThrowIfNull().Any())
        {
            throw new ArgumentException("At least 1 reader is required", nameof(readers));
        }

        _versionedReaders = readers;
    }
    
    public virtual List<TFeature> Read(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(fileName);

        DbaseSchema actualSchema = null;
        var expectedSchemas = new List<DbaseSchema>();

        foreach (var reader in _versionedReaders)
        {
            try
            {
                return reader.Read(entries, featureType, fileName);
            }
            catch (DbaseSchemaMismatchException ex)
            {
                actualSchema = ex.ActualSchema;
                expectedSchemas.Add(ex.ExpectedSchema);
            }
        }

        throw new DbaseReaderNotFoundException(featureType.GetDbfFileName(fileName), actualSchema, expectedSchemas);
    }
}
