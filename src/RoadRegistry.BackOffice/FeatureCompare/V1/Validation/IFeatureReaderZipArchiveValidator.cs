namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Uploads;

public interface IFeatureReaderZipArchiveValidator
{
    Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken);
}
