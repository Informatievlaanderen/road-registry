namespace RoadRegistry.BackOffice.Dbase;

using System;
using System.Collections.Generic;
using System.Linq;
using Uploads;

public interface IVersionedDbaseRecordReader<out TFeature>
{
    TFeature Read(byte[] dbaseRecordBytes, string dbaseSchemaVersion);
}

public class VersionedDbaseRecordReader<TFeature> : IVersionedDbaseRecordReader<TFeature>
    where TFeature : class
{
    private readonly IDictionary<string, IDbaseRecordReader<TFeature>> _versionedReaders;

    public VersionedDbaseRecordReader(params IDbaseRecordReader<TFeature>[] readers)
    {
        if (!readers.ThrowIfNull().Any())
        {
            throw new ArgumentException("At least 1 reader is required", nameof(readers));
        }

        _versionedReaders = readers.ToDictionary(x => x.DbaseSchemaVersion, x => x);
    }

    public virtual TFeature Read(byte[] dbaseRecordBytes, string dbaseSchemaVersion)
    {
        ArgumentNullException.ThrowIfNull(dbaseRecordBytes);
        ArgumentNullException.ThrowIfNull(dbaseSchemaVersion);

        if (_versionedReaders.TryGetValue(dbaseSchemaVersion, out var reader))
        {
            return reader.Read(dbaseRecordBytes);
        }

        throw new DbaseSchemaVersionNotSupportedException(dbaseSchemaVersion);
    }
}
