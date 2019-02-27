namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;

    public interface IZipArchiveEntryTranslator
    {
        TranslatedChanges Translate(ZipArchiveEntry entry, TranslatedChanges changes);
    }
}