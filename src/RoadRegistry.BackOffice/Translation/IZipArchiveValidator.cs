namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;

    public interface IZipArchiveValidator
    {
        ZipArchiveProblems Validate(ZipArchive archive);
    }
}