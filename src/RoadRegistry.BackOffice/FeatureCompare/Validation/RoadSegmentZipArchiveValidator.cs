namespace RoadRegistry.BackOffice.FeatureCompare.Validation;

using Extracts;
using Readers;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using System;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Translators;
using Uploads;

public class RoadSegmentZipArchiveValidator : FeatureReaderZipArchiveValidator<RoadSegmentFeatureCompareAttributes>
{
    private readonly IStreetNameCache _streetNameCache;
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

    public RoadSegmentZipArchiveValidator(RoadSegmentFeatureCompareFeatureReader featureReader, IStreetNameCache streetNameCache)
        : base(FileName, new[] { FeatureType.Extract, FeatureType.Change, FeatureType.Integration },
            featureReader)
    {
        _streetNameCache = streetNameCache.ThrowIfNull();
    }

    public override async Task<ZipArchiveProblems> ValidateAsync(ZipArchive archive, ZipArchiveValidatorContext context, CancellationToken cancellationToken)
    {
        var problems = await base.ValidateAsync(archive, context, cancellationToken);

        if (context.ChangedRoadSegments.Any())
        {
            var streetNameContext = await RoadSegmentFeatureCompareStreetNameContext.FromFeatures(context.ChangedRoadSegments.Values, _streetNameCache, cancellationToken);
            
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

    private static ZipArchiveProblems GetProblemsForStreetNameId(IDbaseFileRecordProblemBuilder recordContext, StreetNameLocalId? id, bool leftSide, RoadSegmentFeatureCompareStreetNameContext streetNameContext)
    {
        var problems = ZipArchiveProblems.None;

        if (id > 0)
        {
            if (streetNameContext.RemovedIds.Contains(id.Value))
            {
                return problems + (leftSide
                    ? recordContext.LeftStreetNameIdIsRemoved(id.Value)
                    : recordContext.RightStreetNameIdIsRemoved(id.Value));
            }

            if (streetNameContext.RenamedIds.TryGetValue(id.Value, out var renamedToId))
            {
                return problems + (leftSide
                    ? recordContext.LeftStreetNameIdIsRenamed(id.Value, renamedToId)
                    : recordContext.RightStreetNameIdIsRenamed(id.Value, renamedToId));
            }
        }

        return problems;
    }
}
