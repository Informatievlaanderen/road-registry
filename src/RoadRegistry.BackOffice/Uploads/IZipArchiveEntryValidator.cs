namespace RoadRegistry.BackOffice.Uploads;

using System.IO.Compression;

public interface IZipArchiveEntryValidator
{
    (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, ZipArchiveValidationContext context);
}