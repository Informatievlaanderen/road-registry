namespace RoadRegistry.BackOffice.Translation
{
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseRecordValidator
    {
        ZipArchiveErrors Validate(ZipArchiveEntry entry, DbaseRecord record);
    }
}
