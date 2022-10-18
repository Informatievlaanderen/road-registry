namespace RoadRegistry.BackOffice.Uploads;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public interface IZipArchiveDbaseRecordsTranslator<TDbaseRecord> where TDbaseRecord : DbaseRecord
{
    TranslatedChanges Translate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TDbaseRecord> records, TranslatedChanges changes);
}