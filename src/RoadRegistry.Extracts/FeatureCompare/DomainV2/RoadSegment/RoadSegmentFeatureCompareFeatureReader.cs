namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Infrastructure.Dbase;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadSegment;
using Schemas.Inwinning.RoadSegments;

public class RoadSegmentFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<RoadSegmentFeatureCompareAttributes>>
{
    private readonly FileEncoding _encoding;
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

    public RoadSegmentFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new InwinningFeatureReader(encoding))
    {
        _encoding = encoding;
    }

    public override (List<Feature<RoadSegmentFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, context);

        problems += archive.ValidateProjectionFileLambert08(featureType, FileName, _encoding);
        problems += archive.ValidateUniqueIdentifiers(features, featureType, FileName, feature => feature.Attributes.TempId);

        switch (featureType)
        {
            case FeatureType.Change:
                problems += archive.ValidateMissingRoadNodes(features, featureType, FileName, context);

                AddToContext(features, featureType, context);
                break;
            case FeatureType.Extract:
                if (context.ZipArchiveMetadata.Inwinning)
                {
                    var excludeProblems = new List<Func<FileProblem, bool>>
                    {
                        p => p.Reason == nameof(DbaseFileProblems.RoadSegmentStatusV2Mismatch) && p.GetParameterValue("Actual") == "-5",
                        p => p.Reason == nameof(DbaseFileProblems.RoadSegmentMorphologyV2Mismatch) && p.GetParameterValue("Actual") == "-113",
                        p => p.Reason == nameof(DbaseFileProblems.RoadSegmentMorphologyV2Mismatch) && p.GetParameterValue("Actual") == "-114",
                        p => p.Reason == nameof(DbaseFileProblems.RoadSegmentMorphologyV2Mismatch) && p.GetParameterValue("Actual") == "-120",
                        p => p.Reason == nameof(DbaseFileProblems.RoadSegmentMorphologyV2Mismatch) && p.GetParameterValue("Actual") == "-8",
                        p => p.Reason == nameof(DbaseFileProblems.RoadSegmentAccessRestrictionV2Mismatch) && p.GetParameterValue("Actual") == "-2",
                        p => p.Reason == nameof(DbaseFileProblems.RoadSegmentAccessRestrictionV2Mismatch) && p.GetParameterValue("Actual") == "-3",
                        p => p.Reason == nameof(DbaseFileProblems.RoadSegmentSurfaceTypeV2Mismatch) && p.GetParameterValue("Actual") == "-8",
                        p => p.Reason == nameof(DbaseFileProblems.RequiredFieldIsNull) && p.GetParameterValue("Field") == "AUTOHEEN",
                        p => p.Reason == nameof(DbaseFileProblems.RequiredFieldIsNull) && p.GetParameterValue("Field") == "AUTOTERUG",
                        p => p.Reason == nameof(DbaseFileProblems.RequiredFieldIsNull) && p.GetParameterValue("Field") == "FIETSHEEN",
                        p => p.Reason == nameof(DbaseFileProblems.RequiredFieldIsNull) && p.GetParameterValue("Field") == "FIETSTERUG",
                        p => p.Reason == nameof(DbaseFileProblems.RequiredFieldIsNull) && p.GetParameterValue("Field") == "VOETGANGER"
                    };
                    problems = ZipArchiveProblems.None + problems
                        .Where(p => excludeProblems.All(x => !x(p)));
                }
                break;
            case FeatureType.Integration:
                problems = ZipArchiveProblems.None + problems
                    .GetMissingOrInvalidFileProblems()
                    .Where(x => !x.File.Equals(featureType.ToProjectionFileName(FileName), StringComparison.InvariantCultureIgnoreCase));

                foreach (var feature in features)
                {
                    if (context.ChangedRoadSegments.TryGetValue(feature.Attributes.TempId, out var knownRoadSegment))
                    {
                        var recordContext = FileName.AtDbaseRecord(featureType, feature.RecordNumber);
                        problems += recordContext.RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange(feature.Attributes.TempId, knownRoadSegment.RecordNumber);
                    }
                }
                break;
        }

        return (features, problems);
    }

    private void AddToContext(List<Feature<RoadSegmentFeatureCompareAttributes>> features, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        if (featureType != FeatureType.Change)
        {
            throw new NotSupportedException($"Only {FeatureType.Change} features can be added to the context");
        }

        foreach (var feature in features)
        {
            if (context.ChangedRoadSegments.ContainsKey(feature.Attributes.TempId))
            {
                continue;
            }

            context.ChangedRoadSegments.Add(feature.Attributes.TempId, feature);
        }
    }

    private sealed class InwinningFeatureReader : ZipArchiveShapeFeatureReader<RoadSegmentDbaseRecord, Feature<RoadSegmentFeatureCompareAttributes>>
    {
        public InwinningFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentFeatureCompareFeatureReader.FileName, RoadSegmentDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadSegmentDbaseRecord dbaseRecord, Geometry geometry, ZipArchiveFeatureReaderContext context)
        {
            return new RecordData
            {
                Geometry = geometry,
                WS_TEMPID = dbaseRecord.WS_TEMPID.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                LBEHEER = dbaseRecord.LBEHEER.GetValue(),
                RBEHEER = dbaseRecord.RBEHEER.GetValue(),
                LSTRNMID = dbaseRecord.LSTRNMID.GetValue(),
                MORF = dbaseRecord.MORF.GetValue(),
                RSTRNMID = dbaseRecord.RSTRNMID.GetValue(),
                STATUS = dbaseRecord.STATUS.GetValue(),
                TOEGANG = dbaseRecord.TOEGANG.GetValue(),
                WEGCAT = dbaseRecord.WEGCAT.GetValue(),
                VERHARDING = dbaseRecord.VERHARDING.GetValue(),
                AUTOHEEN = dbaseRecord.AUTOHEEN.GetValue(),
                AUTOTERUG = dbaseRecord.AUTOTERUG.GetValue(),
                FIETSHEEN = dbaseRecord.FIETSHEEN.GetValue(),
                FIETSTERUG = dbaseRecord.FIETSTERUG.GetValue(),
                VOETGANGER = dbaseRecord.VOETGANGER.GetValue()
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed record RecordData
    {
        public required Geometry Geometry { get; init; }
        public required int? WS_TEMPID { get; init; }
        public required int? WS_OIDN { get; init; }
        public required int? STATUS { get; init; }
        public required int? MORF { get; init; }
        public required string? WEGCAT { get; init; }
        public required int? LSTRNMID { get; init; }
        public required int? RSTRNMID { get; init; }
        public required string? LBEHEER { get; init; }
        public required string? RBEHEER { get; init; }
        public required int? TOEGANG { get; init; }
        public required int? VERHARDING { get; init; }
        public required int? AUTOHEEN { get; init; }
        public required int? AUTOTERUG { get; init; }
        public required int? FIETSHEEN { get; init; }
        public required int? FIETSTERUG { get; init; }
        public required int? VOETGANGER { get; init; }

        public (Feature<RoadSegmentFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(WS_TEMPID), WS_TEMPID);

            var problems = ZipArchiveProblems.None;

            var roadSegmentId = ReadId();

            MultiLineString? ReadGeometry()
            {
                var recordContext = fileName
                    .AtShapeRecord(featureType, recordNumber);

                try
                {
                    var multiLineString = Geometry.ToMultiLineString();

                    var lines = multiLineString
                        .WithMeasureOrdinates()
                        .WithoutDuplicateCoordinates()
                        .Geometries
                        .OfType<LineString>()
                        .ToArray();
                    if (lines.Length != 1)
                    {
                        problems += recordContext.ShapeRecordGeometryLineCountMismatch(1, lines.Length);
                    }
                    else
                    {
                        var line = lines[0];

                        var lineProblems = line.ValidateRoadSegmentGeometry(roadSegmentId);

                        problems += lineProblems.Select(problem => recordContext
                            .Error(problem.Reason)
                            .WithParameters(problem.Parameters.ToArray())
                            .Build());
                    }

                    return multiLineString;
                }
                catch (InvalidCastException)
                {
                    problems += recordContext.ShapeRecordShapeGeometryTypeMismatch(
                        NetTopologySuite.IO.Esri.ShapeType.PolyLineM,
                        Geometry.GeometryType);
                }

                return null;
            }

            RoadSegmentId ReadId()
            {
                if (WS_TEMPID is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WS_TEMPID));
                }
                else if (RoadSegmentId.Accepts(WS_TEMPID.Value))
                {
                    return new RoadSegmentId(WS_TEMPID.Value);
                }
                else
                {
                    problems += problemBuilder.RoadSegmentIdOutOfRange(WS_TEMPID.Value);
                }

                return default;
            }

            RoadSegmentId? ReadRoadSegmentId()
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

                return null;
            }

            RoadSegmentCategoryV2? ReadCategory()
            {
                if (WEGCAT is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WEGCAT));
                }
                else if (RoadSegmentCategoryV2.ByIdentifier.TryGetValue(WEGCAT, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentCategoryV2Mismatch(WEGCAT);
                }

                return null;
            }

            RoadSegmentAccessRestrictionV2? ReadAccessRestriction()
            {
                if (TOEGANG is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(TOEGANG));
                }
                else if (RoadSegmentAccessRestrictionV2.ByIdentifier.TryGetValue(TOEGANG.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentAccessRestrictionV2Mismatch(TOEGANG.Value);
                }

                return null;
            }

            StreetNameLocalId ReadLeftStreetNameId()
            {
                if (LSTRNMID is not null)
                {
                    if (StreetNameLocalId.Accepts(LSTRNMID.Value))
                    {
                        return new StreetNameLocalId(LSTRNMID.Value);
                    }
                    else
                    {
                        problems += problemBuilder.LeftStreetNameIdOutOfRange(LSTRNMID.Value);
                    }
                }

                return StreetNameLocalId.NotApplicable;
            }

            StreetNameLocalId ReadRightStreetNameId()
            {
                if (RSTRNMID is not null)
                {
                    if (StreetNameLocalId.Accepts(RSTRNMID.Value))
                    {
                        return new StreetNameLocalId(RSTRNMID.Value);
                    }
                    else
                    {
                        problems += problemBuilder.RightStreetNameIdOutOfRange(RSTRNMID.Value);
                    }
                }

                return StreetNameLocalId.NotApplicable;
            }

            OrganizationId? ReadLeftMaintenanceAuthority()
            {
                if (string.IsNullOrEmpty(LBEHEER))
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(LBEHEER));
                }
                else if (OrganizationId.AcceptsValue(LBEHEER))
                {
                    return new OrganizationId(LBEHEER);
                }
                else
                {
                    problems += problemBuilder.RoadSegmentMaintenanceAuthorityOutOfRange(LBEHEER);
                }

                return null;
            }

            OrganizationId? ReadRightMaintenanceAuthority()
            {
                if (string.IsNullOrEmpty(RBEHEER))
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(RBEHEER));
                }
                else if (OrganizationId.AcceptsValue(RBEHEER))
                {
                    return new OrganizationId(RBEHEER);
                }
                else
                {
                    problems += problemBuilder.RoadSegmentMaintenanceAuthorityOutOfRange(RBEHEER);
                }

                return null;
            }

            RoadSegmentStatusV2? ReadStatus()
            {
                if (STATUS is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(STATUS));
                }
                else if (RoadSegmentStatusV2.ByIdentifier.TryGetValue(STATUS.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentStatusV2Mismatch(STATUS.Value);
                }

                return null;
            }

            RoadSegmentMorphologyV2? ReadMorphology()
            {
                if (MORF is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(MORF));
                }
                else if (RoadSegmentMorphologyV2.ByIdentifier.TryGetValue(MORF.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentMorphologyV2Mismatch(MORF.Value);
                }

                return null;
            }

            RoadSegmentSurfaceTypeV2? ReadSurfaceType()
            {
                if (VERHARDING is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(VERHARDING));
                }
                else if (RoadSegmentSurfaceTypeV2.ByIdentifier.TryGetValue(VERHARDING.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentSurfaceTypeV2Mismatch(VERHARDING.Value);
                }

                return null;
            }

            bool? ReadCarAccessForward()
            {
                if (AUTOHEEN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(AUTOHEEN));
                }
                else
                {
                    var boolValue = AUTOHEEN.ToBooleanFromDbaseValue();
                    if (boolValue is not null)
                    {
                        return boolValue.Value;
                    }

                    problems += problemBuilder.RoadSegmentAutoHeenMismatch(AUTOHEEN.Value);
                }

                return null;
            }
            bool? ReadCarAccessBackward()
            {
                if (AUTOTERUG is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(AUTOTERUG));
                }
                else
                {
                    var boolValue = AUTOTERUG.ToBooleanFromDbaseValue();
                    if (boolValue is not null)
                    {
                        return boolValue.Value;
                    }

                    problems += problemBuilder.RoadSegmentAutoTerugMismatch(AUTOTERUG.Value);
                }

                return null;
            }
            bool? ReadBikeAccessForward()
            {
                if (FIETSHEEN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(FIETSHEEN));
                }
                else
                {
                    var boolValue = FIETSHEEN.ToBooleanFromDbaseValue();
                    if (boolValue is not null)
                    {
                        return boolValue.Value;
                    }

                    problems += problemBuilder.RoadSegmentFietsHeenMismatch(FIETSHEEN.Value);
                }

                return null;
            }
            bool? ReadBikeAccessBackward()
            {
                if (FIETSTERUG is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(FIETSTERUG));
                }
                else
                {
                    var boolValue = FIETSTERUG.ToBooleanFromDbaseValue();
                    if (boolValue is not null)
                    {
                        return boolValue.Value;
                    }

                    problems += problemBuilder.RoadSegmentFietsTerugMismatch(FIETSTERUG.Value);
                }

                return null;
            }
            bool? ReadPedestrianAccess()
            {
                if (VOETGANGER is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(VOETGANGER));
                }
                else
                {
                    var boolValue = VOETGANGER.ToBooleanFromDbaseValue();
                    if (boolValue is not null)
                    {
                        return boolValue.Value;
                    }

                    problems += problemBuilder.RoadSegmentVoetgangerMismatch(VOETGANGER.Value);
                }

                return null;
            }

            var feature = Feature.New(recordNumber, new RoadSegmentFeatureCompareAttributes
            {
                Geometry = ReadGeometry(),
                TempId = roadSegmentId,
                RoadSegmentId = ReadRoadSegmentId(),
                Method = null,
                StartNodeId = null,
                EndNodeId = null,
                Category = ReadCategory(),
                AccessRestriction = ReadAccessRestriction(),
                LeftSideStreetNameId = ReadLeftStreetNameId(),
                RightSideStreetNameId = ReadRightStreetNameId(),
                LeftMaintenanceAuthorityId = ReadLeftMaintenanceAuthority(),
                RightMaintenanceAuthorityId = ReadRightMaintenanceAuthority(),
                Status = ReadStatus(),
                Morphology = ReadMorphology(),
                SurfaceType = ReadSurfaceType(),
                CarAccessForward = ReadCarAccessForward(),
                CarAccessBackward = ReadCarAccessBackward(),
                BikeAccessForward = ReadBikeAccessForward(),
                BikeAccessBackward = ReadBikeAccessBackward(),
                PedestrianAccess = ReadPedestrianAccess()
            });
            return (feature, problems);
        }
    }
}
