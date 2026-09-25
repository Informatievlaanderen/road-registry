namespace RoadRegistry.Extracts.ZipArchiveWriters;

using RoadRegistry.Extracts;

public interface IZipArchiveWriterFactoryForDomainV2
{
    IZipArchiveWriter Create(string zipArchiveWriterVersion);
}

public class ZipArchiveWriterFactoryForDomainV2 : IZipArchiveWriterFactoryForDomainV2
{
    private readonly IZipArchiveWriter _inwinning;
    private readonly IZipArchiveWriter _bijhouding;

    public ZipArchiveWriterFactoryForDomainV2(
        IZipArchiveWriter inwinning,
        IZipArchiveWriter bijhouding)
    {
        _inwinning = inwinning;
        _bijhouding = bijhouding;
    }

    public IZipArchiveWriter Create(string zipArchiveWriterVersion)
    {
        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV2_Inwinning)
        {
            return _inwinning ?? throw new NotSupportedException();
        }

        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV2_Bijhouding)
        {
            return _bijhouding ?? throw new NotSupportedException();
        }

        throw new NotSupportedException();
    }
}
