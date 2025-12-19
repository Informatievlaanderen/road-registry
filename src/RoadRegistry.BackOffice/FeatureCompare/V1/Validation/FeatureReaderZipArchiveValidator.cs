namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Uploads;
using Translators;

public abstract class FeatureReaderZipArchiveValidator<TAttributes> : IFeatureReaderZipArchiveValidator
    where TAttributes : class
{
    private readonly ExtractFileName _extractFileName;
    private readonly FeatureType[] _featureTypes;
    private readonly IZipArchiveFeatureReader<Feature<TAttributes>> _featureReader;

    protected FeatureReaderZipArchiveValidator(ExtractFileName extractFileName, FeatureType[] featureTypes, IZipArchiveFeatureReader<Feature<TAttributes>> featureReader)
    {
        _extractFileName = extractFileName;
        _featureTypes = featureTypes;
        _featureReader = featureReader;
    }

    public virtual Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(_featureTypes.Aggregate(ZipArchiveProblems.None, (problems, featureType) =>
            problems + _featureReader.Read(archive, featureType, _extractFileName, context).Item2));
    }
}
