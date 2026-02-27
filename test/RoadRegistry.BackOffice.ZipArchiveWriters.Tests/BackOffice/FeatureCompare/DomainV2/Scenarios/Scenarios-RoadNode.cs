namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios;

using Editor.Schema.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using RoadSegment.Changes;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public class RoadNodeScenarios : FeatureCompareTranslatorScenariosBase
{
    public RoadNodeScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task WhenModifiedGeometrySlightly_ThenIdIsKept()
    {
        // Arrange
        var (zipArchive, context) = new DomainV2ZipArchiveBuilder()
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

                builder.TestData.RoadNode2ShapeRecord.Geometry = endPointGeometry;
            })
            .BuildWithContext();

        // Act
        var actualChanges = await TranslateSucceeds(zipArchive);

        // Assert
        actualChanges.Should().HaveCount(2);

        actualChanges.ToList()[0].Should().BeOfType<ModifyRoadNodeChange>();
        actualChanges.ToList()[0].Should().BeEquivalentTo(new ModifyRoadNodeChange
        {
            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
            Geometry = context.Change.TestData.RoadNode2ShapeRecord.Geometry.ToRoadNodeGeometry(),
            Grensknoop = context.Change.TestData.RoadNode2DbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
        });

        actualChanges.ToList()[1].Should().BeOfType<ModifyRoadSegmentChange>();
    }

    [Fact]
    public async Task WhenModifiedGeometryToMoreThanClusterTolerance_ThenExtractNodeIsRemovedAndNewNodeIsAdded()
    {
        var (zipArchive, context) = new DomainV2ZipArchiveBuilder()
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

                builder.TestData.RoadNode2ShapeRecord.Geometry = endPointGeometry;
            })
            .BuildWithContext();

        // Act
        var actualChanges = await TranslateSucceeds(zipArchive);

        // Assert
        actualChanges.Should().HaveCount(3);

        var removeRoadNodeChange = actualChanges.ToList()[0].Should().BeOfType<RemoveRoadNodeChange>().Subject;
        removeRoadNodeChange.RoadNodeId.Should().Be(new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value));

        var addRoadNodeChange = actualChanges.ToList()[1].Should().BeOfType<AddRoadNodeChange>().Subject;
        addRoadNodeChange.TemporaryId.Should().Be(new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value));
        addRoadNodeChange.Geometry.Should().BeEquivalentTo(context.Change.TestData.RoadNode2ShapeRecord.Geometry.ToRoadNodeGeometry());

        actualChanges.ToList()[2].Should().BeOfType<ModifyRoadSegmentChange>();
    }

    [Fact]
    public async Task WhenRecordsWhichAreTooCloseToEachOther_ThenProblem()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
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
    public async Task WhenChangingOnlyTheRoadNodeId_ThenNoChanges()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var maxId = new RoadNodeId(builder.DataSet.RoadNodeDbaseRecords.Max(x => x.WK_OIDN.Value));

                builder.TestData.RoadNode1DbaseRecord.WK_OIDN.Value = maxId.Next();
            })
            .BuildWithResult(context => TranslatedChanges.Empty);

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenChangingTheRoadNodeId_ThenReuseTheRoadNodeIdFromExtractFeature()
    {
        var (zipArchive, context) = new DomainV2ZipArchiveBuilder()
            .WithExtract((builder, context) =>
            {
                var dbaseRecord = builder.CreateRoadNodeDbaseRecord();
                var shapeRecord = builder.CreateRoadNodeShapeRecord();

                builder.DataSet.RoadNodeDbaseRecords.Add(dbaseRecord);
                builder.DataSet.RoadNodeShapeRecords.Add(shapeRecord);
            })
            .WithChange((builder, context) =>
            {
                var dbaseRecord = context.Extract.DataSet.RoadNodeDbaseRecords.Last().Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                var shapeRecord = context.Extract.DataSet.RoadNodeShapeRecords.Last().Clone();

                var maxId = new RoadNodeId(context.Extract.DataSet.RoadNodeDbaseRecords.Max(x => x.WK_OIDN.Value));

                dbaseRecord.WK_OIDN.Value = maxId.Next();
                dbaseRecord.GRENSKNOOP.Value = context.Fixture.CreateWhichIsDifferentThan(dbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()).ToDbaseShortValue();

                builder.DataSet.RoadNodeDbaseRecords.Add(dbaseRecord);
                builder.DataSet.RoadNodeShapeRecords.Add(shapeRecord);
            })
            .BuildWithContext();

        // Act
        var actualChanges = await TranslateSucceeds(zipArchive);

        // Assert
        actualChanges.Should().HaveCount(1);

        var modifyRoadNodeChange = actualChanges.ToList()[0].Should().BeOfType<ModifyRoadNodeChange>().Subject;
        modifyRoadNodeChange.RoadNodeId.Should().Be(new RoadNodeId(context.Extract.DataSet.RoadNodeDbaseRecords.Last().WK_OIDN.Value));
        modifyRoadNodeChange.Grensknoop.Should().Be(context.Change.DataSet.RoadNodeDbaseRecords.Last().GRENSKNOOP.Value.ToBooleanFromDbaseValue());
    }

    [Fact]
    public async Task WhenChangingTheRoadNodeIdAndGeometrySlightly_ThenReuseTheRoadNodeIdFromExtractFeature()
    {
        var (zipArchive, context) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var dbaseRecord = builder.TestData.RoadNode1DbaseRecord;
                var shapeRecord = builder.TestData.RoadNode1ShapeRecord;

                var maxId = new RoadNodeId(builder.DataSet.RoadNodeDbaseRecords.Max(x => x.WK_OIDN.Value));

                dbaseRecord.WK_OIDN.Value = maxId.Next();

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
            .BuildWithContext();

        // Act
        var actualChanges = await TranslateSucceeds(zipArchive);

        // Assert
        actualChanges.Should().HaveCount(2);

        var modifyRoadNodeChange = actualChanges.ToList()[0].Should().BeOfType<ModifyRoadNodeChange>().Subject;
        modifyRoadNodeChange.RoadNodeId.Should().Be(new RoadNodeId(context.Extract.TestData.RoadNode1DbaseRecord.WK_OIDN.Value));
        modifyRoadNodeChange.Geometry.Should().BeEquivalentTo(context.Change.TestData.RoadNode1ShapeRecord.Geometry.ToRoadNodeGeometry());

        actualChanges.ToList()[1].Should().BeOfType<ModifyRoadSegmentChange>();
    }

    [Fact]
    public async Task IdsShouldBeUniqueAcrossChangeAndIntegrationData()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var integrationRoadNode = context.Integration.DataSet.RoadNodeDbaseRecords.First();

                builder.TestData.RoadNode1DbaseRecord.WK_OIDN.Value = integrationRoadNode.WK_OIDN.Value;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange), problem.Reason);
    }
}
