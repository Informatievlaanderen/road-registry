namespace RoadRegistry.BackOffice.Dbase;

using System;
using System.IO;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.IO;

public interface IDbaseRecordReader<out TFeature>
{
    string DbaseSchemaVersion { get; }
    TFeature Read(byte[] dbaseRecordBytes);
}

public abstract class DbaseRecordReader<TDbaseRecord, TFeature> : IDbaseRecordReader<TFeature>
    where TDbaseRecord : DbaseRecord, new()
{
    private readonly Encoding _encoding;
    private readonly RecyclableMemoryStreamManager _manager;

    protected DbaseRecordReader(RecyclableMemoryStreamManager manager, Encoding encoding, string dbaseSchemaVersion)
    {
        _manager = manager;
        _encoding = encoding;
        DbaseSchemaVersion = dbaseSchemaVersion;
    }

    public string DbaseSchemaVersion { get; }

    public TFeature Read(byte[] dbaseRecordBytes)
    {
        ArgumentNullException.ThrowIfNull(dbaseRecordBytes);

        using var input = _manager.GetStream(Guid.NewGuid(), GetType().FullName, dbaseRecordBytes, 0, dbaseRecordBytes.Length);
        using var reader = new BinaryReader(input, _encoding);

        var dbaseRecord = new TDbaseRecord();
        dbaseRecord.Read(reader);

        return Convert(dbaseRecord);
    }

    protected abstract TFeature Convert(TDbaseRecord dbaseRecord);
}
