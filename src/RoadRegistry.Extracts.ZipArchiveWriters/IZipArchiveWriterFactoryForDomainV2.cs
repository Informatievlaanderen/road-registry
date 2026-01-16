namespace RoadRegistry.Extracts.ZipArchiveWriters;

using RoadRegistry.Extracts;

public interface IZipArchiveWriterFactoryForDomainV2
{
    IZipArchiveWriter Create(string zipArchiveWriterVersion);
}

public class ZipArchiveWriterFactoryForDomainV2 : IZipArchiveWriterFactoryForDomainV2
{
    private readonly IZipArchiveWriter _domainV2;

    public ZipArchiveWriterFactoryForDomainV2(
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
