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

        // DomainV2 is what the v1 downloadaanvragen ask for while the UseDomainV2 feature toggle is on, and what
        // extract downloads requested that way carry in the database. It means the same thing as DomainV2_Bijhouding -
        // an extract in the hernieuwde datamodel - and there is no other writer that builds one, so it lands here too
        // rather than on the NotSupportedException it used to get.
        if (zipArchiveWriterVersion is WellKnownZipArchiveWriterVersions.DomainV2
            or WellKnownZipArchiveWriterVersions.DomainV2_Bijhouding)
        {
            return _bijhouding ?? throw new NotSupportedException();
        }

        throw new NotSupportedException();
    }
}
