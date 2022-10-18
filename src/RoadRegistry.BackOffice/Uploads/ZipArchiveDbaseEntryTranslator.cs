namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class ZipArchiveDbaseEntryTranslator<TRecord> : IZipArchiveEntryTranslator
    where TRecord : DbaseRecord, new()
{
    private readonly Encoding _encoding;
    private readonly DbaseFileHeaderReadBehavior _readBehavior;
    private readonly IZipArchiveDbaseRecordsTranslator<TRecord> _translator;

    public ZipArchiveDbaseEntryTranslator(
        Encoding encoding,
        DbaseFileHeaderReadBehavior readBehavior,
        IZipArchiveDbaseRecordsTranslator<TRecord> translator)
    {
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        _readBehavior = readBehavior ?? throw new ArgumentNullException(nameof(readBehavior));
        _translator = translator ?? throw new ArgumentNullException(nameof(translator));
    }

    public TranslatedChanges Translate(ZipArchiveEntry entry, TranslatedChanges changes)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (changes == null) throw new ArgumentNullException(nameof(changes));

        using (var stream = entry.Open())
        using (var reader = new BinaryReader(stream, _encoding))
        {
            var header = DbaseFileHeader.Read(reader, _readBehavior);
            using (var enumerator = header.CreateDbaseRecordEnumerator<TRecord>(reader))
            {
                return _translator.Translate(entry, enumerator, changes);
            }
        }
    }
}