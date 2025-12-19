namespace RoadRegistry.Extracts.Uploads;

using System.IO.Compression;

public interface IZipArchiveEntryValidator
{
    ZipArchiveProblems Validate(ZipArchiveEntry entry);
}
