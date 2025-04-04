namespace RoadRegistry.BackOffice.FeatureCompare.V2.Validation;

using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extracts;
using Translators;
using Uploads;

public abstract class FeatureReaderZipArchiveValidator<TAttributes> : IFeatureReaderZipArchiveValidator
    where TAttributes : class
{
    private readonly FeatureType[] _featureTypes;
    private readonly IZipArchiveFeatureReader<Feature<TAttributes>> _featureReader;

    protected FeatureReaderZipArchiveValidator(FeatureType[] featureTypes, IZipArchiveFeatureReader<Feature<TAttributes>> featureReader)
    {
        _featureTypes = featureTypes;
        _featureReader = featureReader;
    }

    public async virtual Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken)
    {
        return _featureTypes.Aggregate(ZipArchiveProblems.None, (problems, featureType) =>
            problems + _featureReader.Read(archive, featureType, context).Item2);
    }
}
