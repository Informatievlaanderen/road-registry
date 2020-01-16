namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;

    public interface IZipArchiveEntryValidator
    {
        ZipArchiveProblems Validate(ZipArchiveEntry entry);
    }
}
