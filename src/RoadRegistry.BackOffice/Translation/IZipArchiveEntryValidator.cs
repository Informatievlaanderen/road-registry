namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;

    public interface IZipArchiveEntryValidator
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry);
    }
}
