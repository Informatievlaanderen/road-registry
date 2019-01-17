namespace RoadRegistry.Translation
{
    using System.IO.Compression;
    using Aiv.Vbr.Shaperon;

    public interface IZipArchiveDbaseRecordValidator
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, DbaseRecord record);
    }
}