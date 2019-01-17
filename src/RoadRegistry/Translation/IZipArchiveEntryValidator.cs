namespace RoadRegistry.Translation
{
    using System.IO.Compression;

    public interface IZipArchiveEntryValidator
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry);
    }
}