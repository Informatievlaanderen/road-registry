namespace RoadRegistry.BackOffice.Uploads;

using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
//TODO-rik obsolete?
public interface IZipArchiveShapeRecordsValidator
{
    (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records, ZipArchiveValidationContext context);
}

public interface IZipArchiveShapeRecordValidator
{
    (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, RecordNumber recordNumber, Geometry geometry, ZipArchiveValidationContext context);
}
