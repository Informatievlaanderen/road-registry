namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

public interface IZipArchiveWriterFactory
{
    IZipArchiveWriter Create(string zipArchiveWriterVersion);
}

public class ZipArchiveWriterFactory : IZipArchiveWriterFactory
{
    private readonly IZipArchiveWriter _v1;
    private readonly IZipArchiveWriter _v2;

    public ZipArchiveWriterFactory(
        IZipArchiveWriter v1,
        IZipArchiveWriter v2)
    {
        _v1 = v1;
        _v2 = v2;
    }

    public IZipArchiveWriter Create(string zipArchiveWriterVersion)
    {
        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.V2)
        {
            return _v2;
        }

        return _v1 ?? throw new NotSupportedException();
    }
}
