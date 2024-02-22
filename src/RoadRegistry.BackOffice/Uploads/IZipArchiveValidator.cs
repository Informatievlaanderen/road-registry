namespace RoadRegistry.BackOffice.Uploads;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using FeatureCompare;

public interface IZipArchiveValidator
{
    Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken);
}

public class ZipArchiveValidatorContext : ZipArchiveFeatureReaderContext
{
    public ZipArchiveValidatorContext(ZipArchiveMetadata metadata)
        : base(metadata)
    {
    }
}
