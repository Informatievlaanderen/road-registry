namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V3.Scenarios;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema.Extensions;
using Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadRegistry.BackOffice.FeatureCompare.V3;
using RoadRegistry.Tests.BackOffice;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using Uploads;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;
using RoadSegmentSurfaceAttribute = Uploads.RoadSegmentSurfaceAttribute;
using TranslatedChanges = RoadRegistry.BackOffice.FeatureCompare.V3.TranslatedChanges;

public class RoadNodeScenarios : FeatureCompareTranslatorScenariosBase
{
    public RoadNodeScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task ModifiedGeometrySlightly()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var lengthIncrease = 0.01;

                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                var endPointGeometry = new Point(lineString.Coordinates[1].X + lengthIncrease, lineString.Coordinates[1].Y)
                    .WithSrid(lineString.SRID);
                lineString = new LineString([
                        lineString.Coordinates[0],
                        new CoordinateM(endPointGeometry.X, endPointGeometry.Y, lineString.Coordinates[1].M + lengthIncrease)
                    ])
                    .WithSrid(lineString.SRID);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = lineString.Length;

                builder.TestData.RoadNode2ShapeRecord.Geometry = endPointGeometry;
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(new ModifyRoadNodeChange
                {
                    RoadNodeId = new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
                    Geometry = context.Change.TestData.RoadNode2ShapeRecord.Geometry
                })
                .AppendChange(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    OriginalId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    Geometry = context.Change.TestData.RoadSegment1ShapeRecord.Geometry,
                    SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value)),
                        RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value])
                })
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ModifiedGeometryToMoreThanClusterTolerance()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var lengthIncrease = 0.06;

                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                var endPointGeometry = new Point(lineString.Coordinates[1].X + lengthIncrease, lineString.Coordinates[1].Y)
                    .WithSrid(lineString.SRID);
                lineString = new LineString([
                        lineString.Coordinates[0],
                        new CoordinateM(endPointGeometry.X, endPointGeometry.Y, lineString.Coordinates[1].M + lengthIncrease)
                    ])
                    .WithSrid(lineString.SRID);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = lineString.Length;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = lineString.Length;

                builder.TestData.RoadNode2ShapeRecord.Geometry = endPointGeometry;
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(new RemoveRoadNodeChange
                {
                    RoadNodeId = new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value)
                })
                .AppendChange(new AddRoadNodeChange
                {
                    TemporaryId = new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
                    OriginalId = new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
                    Type = RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode2DbaseRecord.TYPE.Value],
                    Geometry = context.Change.TestData.RoadNode2ShapeRecord.Geometry
                })
                .AppendChange(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    OriginalId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    Geometry = context.Change.TestData.RoadSegment1ShapeRecord.Geometry,
                    StartNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                    EndNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                    SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value)),
                        RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value])
                }));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task RecordsWhichAreTooCloseToEachShouldShouldGiveProblem()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var maxId = new RoadNodeId(builder.DataSet.RoadNodeDbaseRecords.Max(x => x.WK_OIDN.Value));

                var nodeDbase = builder.CreateRoadNodeDbaseRecord();
                nodeDbase.WK_OIDN.Value = maxId.Next();

                var node1Geometry = builder.TestData.RoadNode1ShapeRecord.Geometry;
                var nodeShape = builder.CreateRoadNodeShapeRecord(new Point(node1Geometry.X + 0.01, node1Geometry.Y));

                builder.DataSet.RoadNodeDbaseRecords.Add(nodeDbase);
                builder.DataSet.RoadNodeShapeRecords.Add(nodeShape);
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.RoadNodeIsAlreadyProcessed), problem.Reason);
    }

    [Fact]
    public async Task ChangingOnlyTheRoadNodeIdInChangeFeatureShouldResultInNoChanges()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var maxId = new RoadNodeId(builder.DataSet.RoadNodeDbaseRecords.Max(x => x.WK_OIDN.Value));

                builder.TestData.RoadNode1DbaseRecord.WK_OIDN.Value = maxId.Next();

                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = builder.TestData.RoadNode1DbaseRecord.WK_OIDN.Value;
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ChangingTheRoadNodeIdAndTypeInChangeFeatureShouldReuseTheRoadNodeIdFromExtractFeature()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                var dbaseRecord = builder.CreateRoadNodeDbaseRecord();
                var shapeRecord = builder.CreateRoadNodeShapeRecord();

                builder.DataSet.RoadNodeDbaseRecords.Add(dbaseRecord);
                builder.DataSet.RoadNodeShapeRecords.Add(shapeRecord);
            })
            .WithChange((builder, context) =>
            {
                var dbaseRecord = context.Extract.DataSet.RoadNodeDbaseRecords.Last().Clone(new RecyclableMemoryStreamManager(), Encoding);
                var shapeRecord = context.Extract.DataSet.RoadNodeShapeRecords.Last().Clone();

                var maxId = new RoadNodeId(context.Extract.DataSet.RoadNodeDbaseRecords.Max(x => x.WK_OIDN.Value));

                dbaseRecord.WK_OIDN.Value = maxId.Next();
                dbaseRecord.TYPE.Value = context.Fixture.CreateWhichIsDifferentThan(RoadNodeType.ByIdentifier[dbaseRecord.TYPE.Value]).Translation.Identifier;

                builder.DataSet.RoadNodeDbaseRecords.Add(dbaseRecord);
                builder.DataSet.RoadNodeShapeRecords.Add(shapeRecord);
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(new ModifyRoadNodeChange
                {
                    RoadNodeId = new RoadNodeId(context.Extract.DataSet.RoadNodeDbaseRecords.Last().WK_OIDN.Value),
                    Type = RoadNodeType.ByIdentifier[context.Change.DataSet.RoadNodeDbaseRecords.Last().TYPE.Value]
                }));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ChangingTheRoadNodeIdAndTypeInChangeFeatureShouldReuseTheRoadNodeIdFromExtractFeatureAndTheLinkedRoadSegmentShouldAlsoBeUsingTheRoadNodeIdFromExtractFeature()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var dbaseRecord = builder.TestData.RoadNode1DbaseRecord;

                var maxId = new RoadNodeId(builder.DataSet.RoadNodeDbaseRecords.Max(x => x.WK_OIDN.Value));

                dbaseRecord.TYPE.Value = context.Fixture.CreateWhichIsDifferentThan(RoadNodeType.ByIdentifier[dbaseRecord.TYPE.Value]).Translation.Identifier;
                dbaseRecord.WK_OIDN.Value = maxId.Next();
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = dbaseRecord.WK_OIDN.Value;
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(new ModifyRoadNodeChange
                {
                    RoadNodeId = new RoadNodeId(context.Extract.TestData.RoadNode1DbaseRecord.WK_OIDN.Value),
                    Type = RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode1DbaseRecord.TYPE.Value]
                })
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ChangingTheRoadNodeIdAndGeometryInChangeFeatureShouldReuseTheRoadNodeIdFromExtractFeatureAndTheLinkedRoadSegmentShouldAlsoBeUsingTheRoadNodeIdFromExtractFeature()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var dbaseRecord = builder.TestData.RoadNode1DbaseRecord;
                var shapeRecord = builder.TestData.RoadNode1ShapeRecord;

                var maxId = new RoadNodeId(builder.DataSet.RoadNodeDbaseRecords.Max(x => x.WK_OIDN.Value));

                dbaseRecord.WK_OIDN.Value = maxId.Next();
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = dbaseRecord.WK_OIDN.Value;

                var distanceXIncrement = 0.01;
                shapeRecord.Geometry = new Point(shapeRecord.Geometry.X + distanceXIncrement, shapeRecord.Geometry.Y)
                    .WithSrid(shapeRecord.Geometry.SRID);

                var lineString = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
                lineString = new LineString([
                    new CoordinateM(lineString.Coordinates[0].X + distanceXIncrement, lineString.Coordinates[0].Y, lineString.Coordinates[0].M),
                    new CoordinateM(lineString.Coordinates[1].X + distanceXIncrement, lineString.Coordinates[1].Y, lineString.Coordinates[1].M)
                ]).WithSrid(lineString.SRID);
                builder.TestData.RoadSegment1ShapeRecord.Geometry = lineString.ToMultiLineString();
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new ModifyRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Extract.TestData.RoadNode1DbaseRecord.WK_OIDN.Value),
                        Geometry = context.Change.TestData.RoadNode1ShapeRecord.Geometry
                    }
                )
                .AppendChange(
                    new ModifyRoadSegmentChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        OriginalId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Geometry = context.Change.TestData.RoadSegment1ShapeRecord.Geometry,
                        SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>().Add(
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                            new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value)),
                            RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value])
                    })
            );

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task IdsShouldBeUniqueAcrossChangeAndIntegrationData()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var integrationRoadNode = context.Integration.DataSet.RoadNodeDbaseRecords.First();

                builder.TestData.RoadNode1DbaseRecord.WK_OIDN.Value = integrationRoadNode.WK_OIDN.Value;
                builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = builder.TestData.RoadNode1DbaseRecord.WK_OIDN.Value;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange), problem.Reason);
    }
}
