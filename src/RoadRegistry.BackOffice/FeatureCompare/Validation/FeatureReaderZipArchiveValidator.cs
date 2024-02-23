namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Translators;
using Uploads;

public abstract class FeatureReaderZipArchiveValidator<TAttributes> : IZipArchiveValidator
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

    public async virtual Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken)
    {
        return _featureTypes.Aggregate(ZipArchiveProblems.None, (problems, featureType) =>
            problems + _featureReader.Read(archive, featureType, _extractFileName, context).Item2);
    }
}
