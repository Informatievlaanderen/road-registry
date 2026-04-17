namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios.Inwinning;

using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public class AllScenarios : FeatureCompareTranslatorScenariosBase
{
    public AllScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task Added()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(customize: fixture => { fixture.Freeze(RoadSegmentStatusV2.Gepland); })
            .WithExtract((builder, _) =>
            {
                builder.DataSet.RoadNodeDbaseRecords = new[] { builder.TestData.RoadSegment2StartNodeDbaseRecord, builder.TestData.RoadSegment2EndNodeDbaseRecord }.ToList();
                builder.DataSet.RoadNodeShapeRecords = new[] { builder.TestData.RoadSegment2StartNodeShapeRecord, builder.TestData.RoadSegment2EndNodeShapeRecord }.ToList();
                builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment2DbaseRecord }.ToList();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment2ShapeRecord }.ToList();
                builder.DataSet.EuropeanRoadDbaseRecords.Clear();
                builder.DataSet.NationalRoadDbaseRecords.Clear();
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildWithResult(context =>
            {
                var maxRoadSegmentId = context.GetMaxRoadSegmentId();
                var roadSegment1TemporaryId = maxRoadSegmentId.Next();

                return TranslatedChanges.Empty
                    .AppendChange(
                        new AddRoadNodeChange
                        {
                            TemporaryId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                            OriginalId = null,
                            Geometry = context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new AddRoadNodeChange
                        {
                            TemporaryId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                            OriginalId = null,
                            Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        BuildAddRoadSegmentChange(context.Change.TestData.RoadSegment1DbaseRecord, context.Change.TestData.RoadSegment1ShapeRecord) with
                        {
                            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegment1TemporaryId, [new RoadSegmentTempId(context.Change.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value)]),
                            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
                            EuropeanRoadNumbers =
                            [
                                EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!),
                                EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value!)
                            ],
                            NationalRoadNumbers =
                            [
                                NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value!),
                                NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord2.NWNUMMER.Value!)
                            ]
                        }
                    )
                    .AppendChange(
                        new AddGradeSeparatedJunctionChange
                        {
                            TemporaryId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                            Type = GradeSeparatedJunctionTypeV2.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                            UpperRoadSegmentId = roadSegment1TemporaryId,
                            LowerRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value!.Value)
                        }
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task Modified()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(customize: fixture => { fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd); })
            .WithChange((builder, context) =>
            {
                var fixture = context.Fixture;

                builder.TestData.RoadSegment1StartNodeDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(RoadNodeTypeV2.ByIdentifier[builder.TestData.RoadSegment1StartNodeDbaseRecord.TYPE.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value = fixture.CreateWhichIsDifferentThan(builder.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()).ToDbaseShortValue();
                builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry = new Point(builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry.X + 0.01, builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry.Y)
                    .WithSrid(WellknownSrids.Lambert08);

                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatusV2.Gepland;
                builder.TestData.RoadSegment1DbaseRecord.LBEHEER.Value = fixture.CreateWhichIsDifferentThan(new OrganizationId(builder.TestData.RoadSegment1DbaseRecord.LBEHEER.Value!));
                builder.TestData.RoadSegment1DbaseRecord.RBEHEER.Value = fixture.CreateWhichIsDifferentThan(new OrganizationId(builder.TestData.RoadSegment1DbaseRecord.RBEHEER.Value!));
                builder.TestData.RoadSegment1DbaseRecord.MORF.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentMorphologyV2.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.MORF.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.WEGCAT.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentCategoryV2.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.WEGCAT.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.TOEGANG.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentAccessRestrictionV2.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.TOEGANG.Value]).Translation.Identifier;
                builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value = fixture.CreateWhichIsDifferentThan(new StreetNameLocalId(builder.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value!.Value));
                builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value = fixture.CreateWhichIsDifferentThan(new StreetNameLocalId(builder.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value!.Value));
                var lineString = new LineString([
                    new CoordinateM(builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry.X, builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry.Y, builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry.X),
                    new CoordinateM(builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry.X, builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry.Y, builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry.X)
                ]).WithSrid(WellknownSrids.Lambert08);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();

                builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value = fixture.CreateWhichIsDifferentThan(
                    EuropeanRoadNumber.Parse(builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!),
                    EuropeanRoadNumber.Parse(builder.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value!)).ToString();
                builder.TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value = fixture.CreateWhichIsDifferentThan(
                    NationalRoadNumber.Parse(builder.TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value!),
                    NationalRoadNumber.Parse(builder.TestData.RoadSegment1NationalRoadDbaseRecord2.NWNUMMER.Value!)).ToString();
                builder.TestData.RoadSegment1DbaseRecord.VERHARDING.Value = fixture.CreateWhichIsDifferentThan(RoadSegmentSurfaceTypeV2.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.VERHARDING.Value]).Translation.Identifier;
                builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionTypeV2.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
            })
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        BuildModifyRoadSegmentChange(context.Change.TestData.RoadSegment1DbaseRecord, context.Change.TestData.RoadSegment1ShapeRecord) with
                        {
                            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
                        }
                    )
                    .AppendChange(
                        new AddRoadSegmentToEuropeanRoadChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value),
                            Number = EuropeanRoadNumber.Parse(context.Change.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!)
                        }
                    )
                    .AppendChange(
                        new RemoveRoadSegmentFromEuropeanRoadChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value),
                            Number = EuropeanRoadNumber.Parse(context.Extract.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value!)
                        }
                    )
                    .AppendChange(
                        new AddRoadSegmentToNationalRoadChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value),
                            Number = NationalRoadNumber.Parse(context.Change.TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value!)
                        }
                    )
                    .AppendChange(
                        new RemoveRoadSegmentFromNationalRoadChange
                        {
                            RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value),
                            Number = NationalRoadNumber.Parse(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value!)
                        }
                    )
                    .AppendChange(
                        new AddGradeSeparatedJunctionChange
                        {
                            TemporaryId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
                            Type = GradeSeparatedJunctionTypeV2.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
                            UpperRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value),
                            LowerRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value!.Value)
                        }
                    )
                    .AppendChange(
                        new RemoveGradeSeparatedJunctionChange
                        {
                            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                        }
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task NoChanges()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .BuildWithResult(context =>
            {
                return TranslatedChanges.Empty
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2StartNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    )
                    .AppendChange(
                        new ModifyRoadNodeChange
                        {
                            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2EndNodeDbaseRecord.WK_OIDN.Value),
                            Geometry = context.Change.TestData.RoadSegment2EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                            Grensknoop = context.Change.TestData.RoadSegment2EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                        }
                    );
            });

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task Removed()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.DataSet.RoadNodeDbaseRecords = new[] { builder.TestData.RoadSegment2StartNodeDbaseRecord, builder.TestData.RoadSegment2EndNodeDbaseRecord }.ToList();
                builder.DataSet.RoadNodeShapeRecords = new[] { builder.TestData.RoadSegment2StartNodeShapeRecord, builder.TestData.RoadSegment2EndNodeShapeRecord }.ToList();
                builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment2DbaseRecord }.ToList();
                builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment2ShapeRecord }.ToList();
                builder.DataSet.EuropeanRoadDbaseRecords.Clear();
                builder.DataSet.NationalRoadDbaseRecords.Clear();
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new ModifyRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2StartNodeDbaseRecord.WK_OIDN.Value),
                        Geometry = context.Change.TestData.RoadSegment2StartNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                        Grensknoop = context.Change.TestData.RoadSegment2StartNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                    }
                )
                .AppendChange(
                    new ModifyRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment2EndNodeDbaseRecord.WK_OIDN.Value),
                        Geometry = context.Change.TestData.RoadSegment2EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
                        Grensknoop = context.Change.TestData.RoadSegment2EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
                    }
                )
                .AppendChange(
                    new RemoveRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Extract.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value)
                    }
                )
                .AppendChange(
                    new RemoveRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Extract.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value)
                    }
                )
                .AppendChange(
                    new RemoveRoadSegmentChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value!.Value)
                    }
                )
                .AppendChange(
                    new RemoveGradeSeparatedJunctionChange
                    {
                        GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
                    }
                ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }
}
