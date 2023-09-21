namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using Uploads;

public class ZipArchiveBeforeFeatureCompareValidator : IZipArchiveBeforeFeatureCompareValidator
{
    private readonly IZipArchiveValidator[] _validators;

    public ZipArchiveBeforeFeatureCompareValidator(FileEncoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        _validators = new IZipArchiveValidator[]
        {
            new TransactionZoneZipArchiveValidator(encoding),
            new RoadNodeZipArchiveValidator(encoding),
            new RoadSegmentZipArchiveValidator(encoding),
            new RoadSegmentLaneZipArchiveValidator(encoding),
            new RoadSegmentWidthZipArchiveValidator(encoding),
            new RoadSegmentSurfaceZipArchiveValidator(encoding),
            new EuropeanRoadZipArchiveValidator(encoding),
            new NationalRoadZipArchiveValidator(encoding),
            new NumberedRoadZipArchiveValidator(encoding),
            new GradeSeparatedJunctionZipArchiveValidator(encoding)
        };
    }

    public ZipArchiveProblems Validate(ZipArchive archive, ZipArchiveValidatorContext context)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(context);

        var problems = ZipArchiveProblems.None;
        
        foreach (var validator in _validators)
        {
            var validationProblems = validator.Validate(archive, context);
            problems += validationProblems;
        }

        return problems;
    }
}
