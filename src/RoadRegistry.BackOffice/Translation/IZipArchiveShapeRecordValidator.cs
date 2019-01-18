namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;
    using Aiv.Vbr.Shaperon;

    public interface IZipArchiveShapeRecordValidator
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, ShapeRecord record);
    }
}
