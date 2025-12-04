namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2;

public interface IZipArchiveWriterFactory
{
    IZipArchiveWriter Create(string zipArchiveWriterVersion);
}

public class ZipArchiveWriterFactory : IZipArchiveWriterFactory
{
    private readonly IZipArchiveWriter _domainV2;

    public ZipArchiveWriterFactory(
        IZipArchiveWriter domainV2)
    {
        _domainV2 = domainV2;
    }

    public IZipArchiveWriter Create(string zipArchiveWriterVersion)
    {
        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV2)
        {
            return _domainV2 ?? throw new NotSupportedException();
        }

        throw new NotSupportedException();
    }
}
