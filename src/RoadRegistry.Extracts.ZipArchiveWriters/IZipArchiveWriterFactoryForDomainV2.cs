namespace RoadRegistry.Extracts.ZipArchiveWriters;

using RoadRegistry.Extracts;

public interface IZipArchiveWriterFactoryForDomainV2
{
    IZipArchiveWriter Create(string zipArchiveWriterVersion);
}

public class ZipArchiveWriterFactoryForDomainV2 : IZipArchiveWriterFactoryForDomainV2
{
    private readonly IZipArchiveWriter _inwinning;

    public ZipArchiveWriterFactoryForDomainV2(
        IZipArchiveWriter inwinning)
    {
        _inwinning = inwinning;
    }

    public IZipArchiveWriter Create(string zipArchiveWriterVersion)
    {
        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV2_Inwinning)
        {
            return _inwinning ?? throw new NotSupportedException();
        }

        throw new NotSupportedException();
    }
}
