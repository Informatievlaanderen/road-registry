namespace RoadRegistry.BackOffice.Uploads;

using System.IO.Compression;

public interface IZipArchiveTranslator
{
    TranslatedChanges Translate(ZipArchive archive);
}
