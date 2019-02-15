namespace RoadRegistry.BackOffice.Translation
{
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveShapeRecordsValidator
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records);
    }
}
