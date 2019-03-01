namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;

    public interface IZipArchiveValidator
    {
        ZipArchiveErrors Validate(ZipArchive archive);
    }
}