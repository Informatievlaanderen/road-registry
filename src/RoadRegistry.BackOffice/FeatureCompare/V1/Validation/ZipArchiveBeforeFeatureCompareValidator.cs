namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using System;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.BackOffice.Uploads;

public class ZipArchiveBeforeFeatureCompareValidator : IZipArchiveBeforeFeatureCompareValidator
{
    private readonly IFeatureReaderZipArchiveValidator[] _validators;

    public ZipArchiveBeforeFeatureCompareValidator(
        TransactionZoneZipArchiveValidator transactionZoneValidator,
        RoadNodeZipArchiveValidator roadNodeValidator,
        RoadSegmentZipArchiveValidator roadSegmentValidator,
        RoadSegmentLaneZipArchiveValidator roadSegmentLaneValidator,
        RoadSegmentWidthZipArchiveValidator roadSegmentWidthValidator,
        RoadSegmentSurfaceZipArchiveValidator roadSegmentSurfaceValidator,
        EuropeanRoadZipArchiveValidator europeanRoadValidator,
        NationalRoadZipArchiveValidator nationalRoadValidator,
        NumberedRoadZipArchiveValidator numberedRoadValidator,
        GradeSeparatedJunctionZipArchiveValidator gradeSeparatedJunctionValidator
    )
    {
        _validators =
        [
            transactionZoneValidator.ThrowIfNull(),
            roadNodeValidator.ThrowIfNull(),
            roadSegmentValidator.ThrowIfNull(),
            roadSegmentLaneValidator.ThrowIfNull(),
            roadSegmentWidthValidator.ThrowIfNull(),
            roadSegmentSurfaceValidator.ThrowIfNull(),
            europeanRoadValidator.ThrowIfNull(),
            nationalRoadValidator.ThrowIfNull(),
            numberedRoadValidator.ThrowIfNull(),
            gradeSeparatedJunctionValidator.ThrowIfNull()
        ];
    }

    public async Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveMetadata metadata, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(metadata);

        var problems = ZipArchiveProblems.None;
        var context = new ZipArchiveValidatorContext(metadata);

        foreach (var validator in _validators)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var validationProblems = await validator.ValidateAsync(archive, context, cancellationToken);
            problems += validationProblems;
        }

        return problems;
    }
}

public class ZipArchiveValidatorContext: ZipArchiveFeatureReaderContext
{
    public ZipArchiveValidatorContext(ZipArchiveMetadata metadata)
        : base(metadata)
    {
    }
}
