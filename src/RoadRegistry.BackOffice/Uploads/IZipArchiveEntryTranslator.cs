namespace RoadRegistry.BackOffice.Uploads;

using System.IO.Compression;

public interface IZipArchiveEntryTranslator
{
    TranslatedChanges Translate(ZipArchiveEntry entry, TranslatedChanges changes);
}
