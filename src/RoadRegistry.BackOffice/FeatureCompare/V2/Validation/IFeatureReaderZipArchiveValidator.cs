namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.Extracts.Uploads;
using Uploads;

public interface IFeatureReaderZipArchiveValidator
{
    Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken);
}
