namespace RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

using System;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;
using Readers;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;
using RoadRegistry.Extracts.Uploads;
using Translators;

public class RoadSegmentZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentFeatureCompareAttributes>
{
    private readonly IRoadSegmentFeatureCompareStreetNameContextFactory _streetNameContextFactory;
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

    public RoadSegmentZipArchiveValidator(
        RoadSegmentFeatureCompareFeatureReader featureReader,
        IRoadSegmentFeatureCompareStreetNameContextFactory streetNameContextFactory)
        : base(FileName, new[] { FeatureType.Extract, FeatureType.Change, FeatureType.Integration },
            featureReader)
    {
        _streetNameContextFactory = streetNameContextFactory.ThrowIfNull();
    }

    public override async Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken)
    {
        var problems = await base.ValidateAsync(archive, context, cancellationToken);

        if (context.ChangedRoadSegments.Any())
        {
            var streetNameContext = await _streetNameContextFactory.Create(context.ChangedRoadSegments.Values, cancellationToken);

            foreach (var feature in context.ChangedRoadSegments.Values)
            {
                var recordContext = FileName
                    .AtDbaseRecord(FeatureType.Change, feature.RecordNumber)
                    .WithIdentifier(nameof(RoadSegmentDbaseRecord.WS_OIDN), feature.Attributes.Id);

                problems += GetProblemsForStreetNameId(recordContext, feature.Attributes.LeftStreetNameId, true, streetNameContext);
                problems += GetProblemsForStreetNameId(recordContext, feature.Attributes.RightStreetNameId, false, streetNameContext);
            }
        }

        return problems;
    }

    private static ZipArchiveProblems GetProblemsForStreetNameId(IDbaseFileRecordProblemBuilder recordContext, StreetNameLocalId? id, bool leftSide, IRoadSegmentFeatureCompareStreetNameContext streetNameContext)
    {
        var problems = ZipArchiveProblems.None;

        if (id > 0)
        {
            if (!streetNameContext.IsValid(id.Value))
            {
                return problems + (leftSide
                    ? recordContext.LeftStreetNameIdOutOfRange(id.Value)
                    : recordContext.RightStreetNameIdOutOfRange(id.Value));
            }

            if (streetNameContext.IsRemoved(id.Value))
            {
                return problems + (leftSide
                    ? recordContext.LeftStreetNameIdIsRemoved(id.Value)
                    : recordContext.RightStreetNameIdIsRemoved(id.Value));
            }

            if (streetNameContext.TryGetRenamedId(id.Value, out var renamedToId))
            {
                return problems + (leftSide
                    ? recordContext.LeftStreetNameIdIsRenamed(id.Value, renamedToId)
                    : recordContext.RightStreetNameIdIsRenamed(id.Value, renamedToId));
            }
        }

        return problems;
    }
}
