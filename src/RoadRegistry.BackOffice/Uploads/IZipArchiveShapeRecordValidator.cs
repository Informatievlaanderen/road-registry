namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using System.IO.Compression;

public interface IZipArchiveShapeRecordValidator
{
    (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, RecordNumber recordNumber, Geometry geometry, ZipArchiveValidationContext context);
}
