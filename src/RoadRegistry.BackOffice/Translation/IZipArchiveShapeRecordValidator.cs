namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveShapeRecordValidator
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, ShapeRecord record);
    }
}
