namespace RoadRegistry.BackOffice.FeatureCompare.V2.Readers;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using Models;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;
using RoadRegistry.Extracts.Uploads;
using Translators;
using Uploads;
using Validation;
using RoadSegmentWidthAttribute = Core.RoadSegmentWidthAttribute;

public class RoadSegmentWidthFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<RoadSegmentWidthFeatureCompareAttributes>>
{
    private const ExtractFileName FileName = ExtractFileName.AttWegbreedte;

    public RoadSegmentWidthFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding))
    {
    }

    public override (List<Feature<RoadSegmentWidthFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, context);

        problems += archive.ValidateUniqueIdentifiers(features, featureType, FileName, feature => feature.Attributes.Id);

        if (featureType == FeatureType.Change)
        {
            problems = problems.TryToFillMissingFromAndToPositions(features, FileName, context);

            problems += ValidateWidthsPerRoadSegment(features, featureType, context);
            problems += archive.ValidateRoadSegmentsWithoutAttributes(features, FileName, ZipArchiveEntryProblems.RoadSegmentsWithoutWidthAttributes, context);
            problems += archive.ValidateMissingRoadSegments(features, FileName, context);
        }

        return (features, problems);
    }

    private ZipArchiveProblems ValidateWidthsPerRoadSegment(List<Feature<RoadSegmentWidthFeatureCompareAttributes>> features, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var problems = ZipArchiveProblems.None;

        foreach (var roadSegmentGroup in features
                     .Where(x => x.Attributes.RoadSegmentId > 0)
                     .GroupBy(x => x.Attributes.RoadSegmentId))
        {
            var roadSegmentId = roadSegmentGroup.Key;

            if (context.ChangedRoadSegments.TryGetValue(roadSegmentId, out var roadSegmentFeature)
                && roadSegmentFeature.Attributes.Geometry is not null)
            {
                var widths = roadSegmentGroup
                    .Select(feature => feature.Attributes)
                    .Select(width => new RoadSegmentWidthAttribute(
                        width.Id,
                        width.Id,
                        width.Width,
                        width.FromPosition,
                        width.ToPosition,
                        GeometryVersion.Initial))
                    .OrderBy(x => x.From)
                    .ToList();

                if (roadSegmentFeature.Attributes.Geometry.HasExactlyOneLineString())
                {
                    var roadSegmentLine = roadSegmentFeature.Attributes.Geometry.GetSingleLineString();

                    var widthProblems = roadSegmentLine.GetProblemsForRoadSegmentWidths(widths, context.Tolerances);

                    var recordContext = FileName.AtDbaseRecord(featureType, roadSegmentGroup.First().RecordNumber);
                    foreach (var problem in widthProblems)
                    {
                        problems += recordContext
                            .Error(problem.Reason)
                            .WithParameters(problem.Parameters.ToArray())
                            .Build();
                    }
                }
            }
        }

        return problems;
    }

    private sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<RoadSegmentWidthAttributeDbaseRecord, Feature<RoadSegmentWidthFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentWidthFeatureCompareFeatureReader.FileName, RoadSegmentWidthAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentWidthFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadSegmentWidthAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                WB_OIDN = dbaseRecord.WB_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                VANPOS = dbaseRecord.VANPOS.GetValue(),
                TOTPOS = dbaseRecord.TOTPOS.GetValue(),
                BREEDTE = dbaseRecord.BREEDTE.GetValue()
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed class UploadsV2FeatureReader : ZipArchiveDbaseFeatureReader<RoadRegistry.Extracts.Schemas.UploadV2.RoadSegmentWidthAttributeDbaseRecord, Feature<RoadSegmentWidthFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentWidthFeatureCompareFeatureReader.FileName, RoadRegistry.Extracts.Schemas.UploadV2.RoadSegmentWidthAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentWidthFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadRegistry.Extracts.Schemas.UploadV2.RoadSegmentWidthAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                WB_OIDN = dbaseRecord.WB_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                VANPOS = dbaseRecord.VANPOS.GetValue(),
                TOTPOS = dbaseRecord.TOTPOS.GetValue(),
                BREEDTE = dbaseRecord.BREEDTE.GetValue()
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? WB_OIDN { get; init; }
        public int? WS_OIDN { get; init; }
        public double? VANPOS { get; init; }
        public double? TOTPOS { get; init; }
        public int? BREEDTE { get; init; }

        public (Feature<RoadSegmentWidthFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(WB_OIDN), WB_OIDN);

            var problems = ZipArchiveProblems.None;

            AttributeId ReadId()
            {
                if (WB_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WB_OIDN));
                }
                else if (AttributeId.Accepts(WB_OIDN.Value))
                {
                    return new AttributeId(WB_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.IdentifierZero();
                }

                return default;
            }

            RoadSegmentId ReadRoadSegmentId()
            {
                if (WS_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WS_OIDN));
                }
                else if (RoadSegmentId.Accepts(WS_OIDN.Value))
                {
                    return new RoadSegmentId(WS_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.RoadSegmentIdOutOfRange(WS_OIDN.Value);
                }

                return default;
            }

            RoadSegmentPosition ReadFromPosition()
            {
                if (VANPOS is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(VANPOS));
                }
                else if (RoadSegmentPosition.Accepts(VANPOS.Value))
                {
                    if (TOTPOS is not null && VANPOS.Value >= TOTPOS.Value)
                    {
                        problems += problemBuilder.FromPositionEqualToOrGreaterThanToPosition(
                            VANPOS.Value,
                            TOTPOS.Value);
                    }

                    return RoadSegmentPosition.FromDouble(VANPOS.Value);
                }
                else
                {
                    problems += problemBuilder.FromPositionOutOfRange(VANPOS.Value);
                }

                return default;
            }

            RoadSegmentPosition ReadToPosition()
            {
                if (TOTPOS is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(TOTPOS));
                }
                else if (RoadSegmentPosition.Accepts(TOTPOS.Value))
                {
                    return RoadSegmentPosition.FromDouble(TOTPOS.Value);
                }
                else
                {
                    problems += problemBuilder.ToPositionOutOfRange(TOTPOS.Value);
                }

                return default;
            }

            RoadSegmentWidth ReadWidth()
            {
                if (BREEDTE is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(BREEDTE));
                }
                else if (RoadSegmentWidth.Accepts(BREEDTE.Value))
                {
                    return new RoadSegmentWidth(BREEDTE.Value);
                }
                else
                {
                    problems += problemBuilder.WidthOutOfRange(BREEDTE.Value);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new RoadSegmentWidthFeatureCompareAttributes
            {
                Id = ReadId(),
                RoadSegmentId = ReadRoadSegmentId(),
                FromPosition = ReadFromPosition(),
                ToPosition = ReadToPosition(),
                Width = ReadWidth()
            });
            return (feature, problems);
        }
    }
}
