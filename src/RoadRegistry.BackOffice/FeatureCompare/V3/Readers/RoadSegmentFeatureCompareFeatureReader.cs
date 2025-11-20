namespace RoadRegistry.BackOffice.FeatureCompare.V3.Readers;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Models;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Uploads;
using RoadSegment;
using RoadSegment.ValueObjects;
using Translators;
using V3;
using Validation;

public class RoadSegmentFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<RoadSegmentFeatureCompareAttributes>>
{
    private readonly FileEncoding _encoding;
    private const ExtractFileName FileName = ExtractFileName.Wegsegment;

    public RoadSegmentFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding))
    {
        _encoding = encoding;
    }

    public override (List<Feature<RoadSegmentFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, context);

        problems += archive.ValidateProjectionFile(featureType, FileName, _encoding);
        problems += archive.ValidateUniqueIdentifiers(features, featureType, FileName, feature => feature.Attributes.Id);

        switch (featureType)
        {
            case FeatureType.Change:
                problems += archive.ValidateMissingRoadNodes(features, featureType, FileName, context);

                AddToContext(features, featureType, context);
                break;
            case FeatureType.Integration:
                problems = ZipArchiveProblems.None + problems
                    .GetMissingOrInvalidFileProblems()
                    .Where(x => !x.File.Equals(featureType.ToProjectionFileName(FileName), StringComparison.InvariantCultureIgnoreCase));

                foreach (var feature in features)
                {
                    if (context.ChangedRoadSegments.TryGetValue(feature.Attributes.Id, out var knownRoadSegment))
                    {
                        var recordContext = FileName.AtDbaseRecord(featureType, feature.RecordNumber);
                        problems += recordContext.RoadSegmentIdentifierNotUniqueAcrossIntegrationAndChange(feature.Attributes.Id, knownRoadSegment.RecordNumber);
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
            if (context.ChangedRoadSegments.ContainsKey(feature.Attributes.Id))
            {
                continue;
            }

            context.ChangedRoadSegments.Add(feature.Attributes.Id, feature);
        }
    }

    private sealed class ExtractsFeatureReader : ZipArchiveShapeFeatureReader<RoadSegmentDbaseRecord, Feature<RoadSegmentFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentFeatureCompareFeatureReader.FileName, RoadSegmentDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadSegmentDbaseRecord dbaseRecord, Geometry geometry, ZipArchiveFeatureReaderContext context)
        {
            return new RecordData
            {
                B_WK_OIDN = dbaseRecord.B_WK_OIDN.GetValue(),
                BEHEER = dbaseRecord.BEHEER.GetValue(),
                E_WK_OIDN = dbaseRecord.E_WK_OIDN.GetValue(),
                LSTRNMID = dbaseRecord.LSTRNMID.GetValue(),
                METHODE = dbaseRecord.METHODE.GetValue(),
                MORF = dbaseRecord.MORF.GetValue(),
                RSTRNMID = dbaseRecord.RSTRNMID.GetValue(),
                STATUS = dbaseRecord.STATUS.GetValue(),
                TGBEP = dbaseRecord.TGBEP.GetValue(),
                WEGCAT = dbaseRecord.WEGCAT.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                Geometry = geometry
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed class UploadsV2FeatureReader : ZipArchiveShapeFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord, Feature<RoadSegmentFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentFeatureCompareFeatureReader.FileName, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord.Schema)
        {
        }

        protected override (Feature<RoadSegmentFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord dbaseRecord, Geometry geometry, ZipArchiveFeatureReaderContext context)
        {
            return new RecordData
            {
                B_WK_OIDN = dbaseRecord.B_WK_OIDN.GetValue(),
                BEHEER = dbaseRecord.BEHEER.GetValue(),
                E_WK_OIDN = dbaseRecord.E_WK_OIDN.GetValue(),
                LSTRNMID = dbaseRecord.LSTRNMID.GetValue(),
                METHODE = dbaseRecord.METHODE.GetValue(),
                MORF = dbaseRecord.MORF.GetValue(),
                RSTRNMID = dbaseRecord.RSTRNMID.GetValue(),
                STATUS = dbaseRecord.STATUS.GetValue(),
                TGBEP = dbaseRecord.TGBEP.GetValue(),
                WEGCAT = dbaseRecord.WEGCAT.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                Geometry = geometry
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed record RecordData
    {
        public int? WS_OIDN { get; init; }
        public int? B_WK_OIDN { get; init; }
        public string BEHEER { get; init; }
        public int? E_WK_OIDN { get; init; }
        public int? LSTRNMID { get; init; }
        public int? METHODE { get; init; }
        public int? MORF { get; init; }
        public int? RSTRNMID { get; init; }
        public int? STATUS { get; init; }
        public int? TGBEP { get; init; }
        public string WEGCAT { get; init; }
        public Geometry Geometry { get; init; }

        public (Feature<RoadSegmentFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(WS_OIDN), WS_OIDN);

            var problems = ZipArchiveProblems.None;

            var roadSegmentId = ReadId();

            RoadSegmentId ReadId()
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

            RoadSegmentGeometryDrawMethod ReadMethod()
            {
                if (METHODE is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(METHODE));
                }
                else if (RoadSegmentGeometryDrawMethod.ByIdentifier.TryGetValue(METHODE.Value, out var value)
                         && value.IsAllowed())
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentGeometryDrawMethodMismatch(METHODE.Value);
                }

                return default;
            }

            RoadSegmentCategory ReadCategory()
            {
                if (WEGCAT is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WEGCAT));
                }
                else if (RoadSegmentCategory.ByIdentifier.TryGetValue(WEGCAT, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentCategoryMismatch(WEGCAT);
                }

                return default;
            }

            RoadSegmentAccessRestriction ReadAccessRestriction()
            {
                if (TGBEP is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(TGBEP));
                }
                else if (RoadSegmentAccessRestriction.ByIdentifier.TryGetValue(TGBEP.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentAccessRestrictionMismatch(TGBEP.Value);
                }

                return default;
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

            OrganizationId ReadMaintenanceAuthority()
            {
                if (string.IsNullOrEmpty(BEHEER))
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(BEHEER));
                }
                else if (OrganizationId.AcceptsValue(BEHEER))
                {
                    return new OrganizationId(BEHEER);
                }
                else
                {
                    problems += problemBuilder.RoadSegmentMaintenanceAuthorityOutOfRange(BEHEER);
                }

                return default;
            }

            RoadSegmentStatus ReadStatus(RoadSegmentGeometryDrawMethod method)
            {
                var outlined = method == RoadSegmentGeometryDrawMethod.Outlined;

                if (STATUS is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(STATUS));
                }
                else if (RoadSegmentStatus.ByIdentifier.TryGetValue(STATUS.Value, out var value)
                         && value.IsValid(outlined))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentStatusMismatch(STATUS.Value, outlined);
                }

                return default;
            }

            RoadSegmentMorphology ReadMorphology(RoadSegmentGeometryDrawMethod method)
            {
                var outlined = method == RoadSegmentGeometryDrawMethod.Outlined;

                if (MORF is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(MORF));
                }
                else if (RoadSegmentMorphology.ByIdentifier.TryGetValue(MORF.Value, out var value)
                         && value.IsValid(outlined))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.RoadSegmentMorphologyMismatch(MORF.Value, outlined);
                }

                return default;
            }

            RoadNodeId ReadStartNodeId(RoadSegmentGeometryDrawMethod method)
            {
                if (method == RoadSegmentGeometryDrawMethod.Outlined)
                {
                    if (B_WK_OIDN is not null)
                    {
                        if (RoadNodeId.Accepts(B_WK_OIDN.Value) && B_WK_OIDN.Value.IsValidStartRoadNodeIdForRoadSegmentOutline())
                        {
                            return new RoadNodeId(B_WK_OIDN.Value);
                        }
                        else
                        {
                            problems += problemBuilder.BeginRoadNodeIdOutOfRange(B_WK_OIDN.Value);
                        }
                    }
                }
                else
                {
                    if (B_WK_OIDN is null)
                    {
                        problems += problemBuilder.RequiredFieldIsNull(nameof(B_WK_OIDN));
                    }
                    else if (RoadNodeId.Accepts(B_WK_OIDN.Value))
                    {
                        if (E_WK_OIDN.HasValue && B_WK_OIDN.Value.Equals(E_WK_OIDN.Value))
                        {
                            problems += problemBuilder.BeginRoadNodeIdEqualsEndRoadNode(B_WK_OIDN.Value, E_WK_OIDN.Value);
                        }
                        else
                        {
                            return new RoadNodeId(B_WK_OIDN.Value);
                        }
                    }
                    else
                    {
                        problems += problemBuilder.BeginRoadNodeIdOutOfRange(B_WK_OIDN.Value);
                    }
                }

                return default;
            }

            RoadNodeId ReadEndNodeId(RoadSegmentGeometryDrawMethod method)
            {
                if (method == RoadSegmentGeometryDrawMethod.Outlined)
                {
                    if (E_WK_OIDN is not null)
                    {
                        if (RoadNodeId.Accepts(E_WK_OIDN.Value) && E_WK_OIDN.Value.IsValidEndRoadNodeIdForRoadSegmentOutline())
                        {
                            return new RoadNodeId(E_WK_OIDN.Value);
                        }
                        else
                        {
                            problems += problemBuilder.EndRoadNodeIdOutOfRange(E_WK_OIDN.Value);
                        }
                    }
                }
                else
                {
                    if (E_WK_OIDN is null)
                    {
                        problems += problemBuilder.RequiredFieldIsNull(nameof(E_WK_OIDN));
                    }
                    else if (RoadNodeId.Accepts(E_WK_OIDN.Value))
                    {
                        return new RoadNodeId(E_WK_OIDN.Value);
                    }
                    else
                    {
                        problems += problemBuilder.EndRoadNodeIdOutOfRange(E_WK_OIDN.Value);
                    }
                }

                return default;
            }

            MultiLineString ReadGeometry()
            {
                var recordContext = fileName
                    .AtShapeRecord(featureType, recordNumber);

                try
                {
                    var multiLineString = Geometry.ToMultiLineString();

                    var lines = multiLineString
                        .WithMeasureOrdinates()
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
                        Geometry?.GeometryType);
                }

                return null;
            }

            var method = ReadMethod();

            var feature = Feature.New(recordNumber, new RoadSegmentFeatureCompareAttributes
            {
                Id = roadSegmentId,
                Method = method,
                Category = ReadCategory(),
                AccessRestriction = ReadAccessRestriction(),
                LeftSideStreetNameId = ReadLeftStreetNameId(),
                RightSideStreetNameId = ReadRightStreetNameId(),
                MaintenanceAuthority = ReadMaintenanceAuthority(),
                Status = ReadStatus(method),
                Morphology = ReadMorphology(method),
                StartNodeId = ReadStartNodeId(method),
                EndNodeId = ReadEndNodeId(method),
                Geometry = ReadGeometry()
            });
            return (feature, problems);
        }
    }
}
