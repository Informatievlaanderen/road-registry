namespace RoadRegistry.BackOffice.FeatureCompare.V1.Readers;

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using Extracts.Dbase.RoadSegments;
using Models;
using NetTopologySuite.Geometries;
using Translators;
using Uploads;
using Validation;
using RoadSegmentLaneAttribute = Core.RoadSegmentLaneAttribute;

public class RoadSegmentLaneFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<RoadSegmentLaneFeatureCompareAttributes>>
{
    public RoadSegmentLaneFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding))
    {
    }

    public override (List<Feature<RoadSegmentLaneFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, fileName, context);

        problems += archive.ValidateUniqueIdentifiers(features, featureType, fileName, feature => feature.Attributes.Id);

        if (featureType == FeatureType.Change)
        {
            problems = problems.TryToFillMissingFromAndToPositions(features, fileName, context);

            problems += ValidateLanesPerRoadSegment(features, fileName, context);
            problems += archive.ValidateRoadSegmentsWithoutAttributes(features, fileName, ZipArchiveEntryProblems.RoadSegmentsWithoutLaneAttributes, context);
            problems += archive.ValidateMissingRoadSegments(features, fileName, context);
        }

        return (features, problems);
    }

    private ZipArchiveProblems ValidateLanesPerRoadSegment(List<Feature<RoadSegmentLaneFeatureCompareAttributes>> features, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var problems = ZipArchiveProblems.None;
        var featureType = FeatureType.Change;

        foreach (var roadSegmentGroup in features
                     .Where(x => x.Attributes.RoadSegmentId > 0)
                     .GroupBy(x => x.Attributes.RoadSegmentId))
        {
            var roadSegmentId = roadSegmentGroup.Key;

            if (context.ChangedRoadSegments.TryGetValue(roadSegmentId, out var roadSegmentFeature)
                && roadSegmentFeature.Attributes.Geometry is not null)
            {
                var lanes = roadSegmentGroup
                    .Select(feature => feature.Attributes)
                    .Select(lane => new RoadSegmentLaneAttribute(
                        lane.Id,
                        lane.Id,
                        lane.Count,
                        lane.Direction,
                        lane.FromPosition,
                        lane.ToPosition,
                        GeometryVersion.Initial))
                    .OrderBy(x => x.From)
                    .ToList();

                if (roadSegmentFeature.Attributes.Geometry.HasExactlyOneLineString())
                {
                    var roadSegmentLine = roadSegmentFeature.Attributes.Geometry.GetSingleLineString();

                    var laneProblems = roadSegmentLine.GetProblemsForRoadSegmentLanes(lanes, context.Tolerances);

                    var recordContext = fileName.AtDbaseRecord(featureType, roadSegmentGroup.First().RecordNumber);
                    foreach (var problem in laneProblems)
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

    public sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<RoadSegmentLaneAttributeDbaseRecord, Feature<RoadSegmentLaneFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentLaneAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentLaneFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, RoadSegmentLaneAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                RS_OIDN = dbaseRecord.RS_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                VANPOS = dbaseRecord.VANPOS.GetValue(),
                TOTPOS = dbaseRecord.TOTPOS.GetValue(),
                AANTAL = dbaseRecord.AANTAL.GetValue(),
                RICHTING = dbaseRecord.RICHTING.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    public sealed class UploadsV2FeatureReader : ZipArchiveDbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord, Feature<RoadSegmentLaneFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentLaneFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                RS_OIDN = dbaseRecord.RS_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                VANPOS = dbaseRecord.VANPOS.GetValue(),
                TOTPOS = dbaseRecord.TOTPOS.GetValue(),
                AANTAL = dbaseRecord.AANTAL.GetValue(),
                RICHTING = dbaseRecord.RICHTING.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? RS_OIDN { get; init; }
        public int? WS_OIDN { get; init; }
        public double? VANPOS { get; init; }
        public double? TOTPOS { get; init; }
        public int? AANTAL { get; init; }
        public int? RICHTING { get; init; }

        public (Feature<RoadSegmentLaneFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(RS_OIDN), RS_OIDN);

            var problems = ZipArchiveProblems.None;

            AttributeId ReadId()
            {
                if (RS_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(RS_OIDN));
                }
                else if (AttributeId.Accepts(RS_OIDN.Value))
                {
                    return new AttributeId(RS_OIDN.Value);
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

            RoadSegmentLaneCount ReadCount()
            {
                if (AANTAL is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(AANTAL));
                }
                else if (RoadSegmentLaneCount.Accepts(AANTAL.Value))
                {
                    return new RoadSegmentLaneCount(AANTAL.Value);
                }
                else
                {
                    problems += problemBuilder.LaneCountOutOfRange(AANTAL.Value);
                }

                return default;
            }

            RoadSegmentLaneDirection ReadDirection()
            {
                if (RICHTING is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(RICHTING));
                }
                else if (RoadSegmentLaneDirection.ByIdentifier.TryGetValue(RICHTING.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.LaneDirectionMismatch(RICHTING.Value);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new RoadSegmentLaneFeatureCompareAttributes
            {
                Id = ReadId(),
                RoadSegmentId = ReadRoadSegmentId(),
                FromPosition = ReadFromPosition(),
                ToPosition = ReadToPosition(),
                Count = ReadCount(),
                Direction = ReadDirection()
            });
            return (feature, problems);
        }
    }
}
