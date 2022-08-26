namespace RoadRegistry.BackOffice.Uploads
{
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseEntryValidator : IZipArchiveEntryValidator
    {
        Encoding Encoding { get; }
        DbaseSchema Schema { get; }
        DbaseFileHeaderReadBehavior HeaderReadBehavior { get; }
    }
}
