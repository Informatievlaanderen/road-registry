namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;

    public interface IZipArchiveTranslator
    {
        TranslatedChanges Translate(ZipArchive archive);
    }
}