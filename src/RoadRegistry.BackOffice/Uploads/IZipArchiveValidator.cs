namespace RoadRegistry.BackOffice.Uploads
{
    using System.IO.Compression;

    public interface IZipArchiveValidator
    {
        ZipArchiveProblems Validate(ZipArchive archive);
    }
}
