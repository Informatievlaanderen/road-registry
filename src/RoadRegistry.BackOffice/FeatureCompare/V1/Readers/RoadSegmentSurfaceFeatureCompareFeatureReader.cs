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
using RoadSegmentSurfaceAttribute = Core.RoadSegmentSurfaceAttribute;

public class RoadSegmentSurfaceFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<RoadSegmentSurfaceFeatureCompareAttributes>>
{
    public RoadSegmentSurfaceFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    public override (List<Feature<RoadSegmentSurfaceFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, fileName, context);

        problems += archive.ValidateUniqueIdentifiers(features, featureType, fileName, feature => feature.Attributes.Id);

        if (featureType == FeatureType.Change)
        {
            problems = problems.TryToFillMissingFromAndToPositions(features, fileName, context);

            problems += ValidateSurfacesPerRoadSegment(features, fileName, context);
            problems += archive.ValidateRoadSegmentsWithoutAttributes(features, fileName, ZipArchiveEntryProblems.RoadSegmentsWithoutSurfaceAttributes, context);
            problems += archive.ValidateMissingRoadSegments(features, fileName, context);
        }

        return (features, problems);
    }

    private ZipArchiveProblems ValidateSurfacesPerRoadSegment(List<Feature<RoadSegmentSurfaceFeatureCompareAttributes>> features, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
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
                var surfaces = roadSegmentGroup
                    .Select(feature => feature.Attributes)
                    .Select(surface => new RoadSegmentSurfaceAttribute(
                        surface.Id,
                        surface.Id,
                        surface.Type,
                        surface.FromPosition,
                        surface.ToPosition,
                        GeometryVersion.Initial))
                    .OrderBy(x => x.From)
                    .ToList();

                if (roadSegmentFeature.Attributes.Geometry.HasExactlyOneLineString())
                {
                    var roadSegmentLine = roadSegmentFeature.Attributes.Geometry.GetSingleLineString();

                    var surfaceProblems = roadSegmentLine.GetProblemsForRoadSegmentSurfaces(surfaces, context.Tolerances);

                    var recordContext = fileName.AtDbaseRecord(featureType, roadSegmentGroup.First().RecordNumber);
                    foreach (var problem in surfaceProblems)
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

    private sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<RoadSegmentSurfaceAttributeDbaseRecord, Feature<RoadSegmentSurfaceFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentSurfaceAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentSurfaceFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                WV_OIDN = dbaseRecord.WV_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                VANPOS = dbaseRecord.VANPOS.GetValue(),
                TOTPOS = dbaseRecord.TOTPOS.GetValue(),
                TYPE = dbaseRecord.TYPE.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed class UploadsV2FeatureReader : ZipArchiveDbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord, Feature<RoadSegmentSurfaceFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentSurfaceFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                WV_OIDN = dbaseRecord.WV_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                VANPOS = dbaseRecord.VANPOS.GetValue(),
                TOTPOS = dbaseRecord.TOTPOS.GetValue(),
                TYPE = dbaseRecord.TYPE.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed class UploadsV1FeatureReader : ZipArchiveDbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord, Feature<RoadSegmentSurfaceFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentSurfaceFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                WV_OIDN = dbaseRecord.WV_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                VANPOS = dbaseRecord.VANPOS.GetValue(),
                TOTPOS = dbaseRecord.TOTPOS.GetValue(),
                TYPE = dbaseRecord.TYPE.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? WV_OIDN { get; init; }
        public int? WS_OIDN { get; init; }
        public double? VANPOS { get; init; }
        public double? TOTPOS { get; init; }
        public int? TYPE { get; init; }

        public (Feature<RoadSegmentSurfaceFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(WV_OIDN), WV_OIDN);

            var problems = ZipArchiveProblems.None;

            AttributeId ReadId()
            {
                if (WV_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WV_OIDN));
                }
                else if (AttributeId.Accepts(WV_OIDN.Value))
                {
                    return new AttributeId(WV_OIDN.Value);
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

            RoadSegmentSurfaceType ReadType()
            {
                if (TYPE is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(TYPE));
                }
                else if (RoadSegmentSurfaceType.ByIdentifier.TryGetValue(TYPE.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.SurfaceTypeMismatch(TYPE.Value);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new RoadSegmentSurfaceFeatureCompareAttributes
            {
                Id = ReadId(),
                RoadSegmentId = ReadRoadSegmentId(),
                FromPosition = ReadFromPosition(),
                ToPosition = ReadToPosition(),
                Type = ReadType()
            });
            return (feature, problems);
        }
    }
}
