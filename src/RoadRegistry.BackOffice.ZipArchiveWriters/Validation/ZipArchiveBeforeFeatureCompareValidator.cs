namespace RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

using System.IO.Compression;
using FeatureCompare.Translators;
using Uploads;

public class ZipArchiveBeforeFeatureCompareValidator : IZipArchiveBeforeFeatureCompareValidator
{
    private readonly IZipArchiveValidator[] _validators;

    public ZipArchiveBeforeFeatureCompareValidator(
        TransactionZoneFeatureCompareFeatureReader transactionZoneFeatureReader,
        RoadNodeFeatureCompareFeatureReader roadNodeFeatureReader,
        RoadSegmentFeatureCompareFeatureReader roadSegmentFeatureReader,
        RoadSegmentLaneFeatureCompareFeatureReader roadSegmentLaneFeatureReader,
        RoadSegmentWidthFeatureCompareFeatureReader roadSegmentWidthFeatureReader,
        RoadSegmentSurfaceFeatureCompareFeatureReader roadSegmentSurfaceFeatureReader,
        EuropeanRoadFeatureCompareFeatureReader europeanRoadFeatureReader,
        NationalRoadFeatureCompareFeatureReader nationalRoadFeatureReader,
        NumberedRoadFeatureCompareFeatureReader numberedRoadFeatureReader,
        GradeSeparatedJunctionFeatureCompareFeatureReader gradeSeparatedJunctionFeatureReader
    )
    {
        _validators = new IZipArchiveValidator[]
        {
            new TransactionZoneZipArchiveValidator(transactionZoneFeatureReader.ThrowIfNull()),
            new RoadNodeZipArchiveValidator(roadNodeFeatureReader.ThrowIfNull()),
            new RoadSegmentZipArchiveValidator(roadSegmentFeatureReader.ThrowIfNull()),
            new RoadSegmentLaneZipArchiveValidator(roadSegmentLaneFeatureReader.ThrowIfNull()),
            new RoadSegmentWidthZipArchiveValidator(roadSegmentWidthFeatureReader.ThrowIfNull()),
            new RoadSegmentSurfaceZipArchiveValidator(roadSegmentSurfaceFeatureReader.ThrowIfNull()),
            new EuropeanRoadZipArchiveValidator(europeanRoadFeatureReader.ThrowIfNull()),
            new NationalRoadZipArchiveValidator(nationalRoadFeatureReader.ThrowIfNull()),
            new NumberedRoadZipArchiveValidator(numberedRoadFeatureReader.ThrowIfNull()),
            new GradeSeparatedJunctionZipArchiveValidator(gradeSeparatedJunctionFeatureReader.ThrowIfNull())
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
