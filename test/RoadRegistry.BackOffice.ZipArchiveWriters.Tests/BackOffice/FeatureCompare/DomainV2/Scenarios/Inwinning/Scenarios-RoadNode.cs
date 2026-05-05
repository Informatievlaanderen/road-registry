namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios.Inwinning;

using FluentAssertions;
using JasperFx.Core;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using RoadRegistry.Editor.Schema.Extensions;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
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
    public async Task WhenEmptyDbfOrShp_ThenError()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.DataSet.RoadNodeDbaseRecords.Clear();
                builder.DataSet.RoadNodeShapeRecords.Clear();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        ex.Problems.Should().Contain(x => x.File == "WEGKNOOP.DBF" && x.Reason == nameof(DbaseFileProblems.HasNoDbaseRecords));
        ex.Problems.Should().Contain(x => x.File == "WEGKNOOP.SHP" && x.Reason == nameof(ShapeFileProblems.HasNoShapeRecords));
    }

    [Fact]
    public async Task WhenModifiedGeometrySlightly_ThenIdIsKept()
    {
        // Arrange
        var (zipArchive, context) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, _) =>
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

                builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry = endPointGeometry;
            })
            .BuildWithContext();

        // Act
        var actualChanges = await TranslateSucceeds(zipArchive);

        // Assert
        actualChanges.Should().ContainEquivalentOf(new ModifyRoadNodeChange
        {
            RoadNodeId = new RoadNodeId(context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value),
            Geometry = context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry(),
            Grensknoop = context.Change.TestData.RoadSegment1EndNodeDbaseRecord.GRENSKNOOP.Value.ToBooleanFromDbaseValue()
        });

        actualChanges.Should().Contain(x => x is ModifyRoadSegmentChange);
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

                builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry = endPointGeometry;
            })
            .BuildWithContext();

        // Act
        var actualChanges = await TranslateSucceeds(zipArchive);

        // Assert
        actualChanges.Should().Contain(x => x is RemoveRoadNodeChange
                                            && ((RemoveRoadNodeChange)x).RoadNodeId == context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value);

        actualChanges.Should().Contain(x => x is AddRoadNodeChange
                                            && ((AddRoadNodeChange)x).TemporaryId == context.Change.TestData.RoadSegment1EndNodeDbaseRecord.WK_OIDN.Value
                                            && ((AddRoadNodeChange)x).Geometry == context.Change.TestData.RoadSegment1EndNodeShapeRecord.Geometry.ToRoadNodeGeometry());

        actualChanges.Should().Contain(x => x is ModifyRoadSegmentChange);
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

                var node1Geometry = builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry;
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

                builder.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value = maxId.Next();
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new ModifyRoadNodeChange
                    {
                        RoadNodeId = new RoadNodeId(context.Extract.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value),
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
                ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task WhenChangingTheRoadNodeId_ThenReuseTheRoadNodeIdFromExtractFeature()
    {
        var (zipArchive, context) = new DomainV2ZipArchiveBuilder()
            .WithExtract((builder, _) =>
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
        actualChanges.Should().Contain(x => x is ModifyRoadNodeChange
            && ((ModifyRoadNodeChange)x).RoadNodeId == context.Extract.DataSet.RoadNodeDbaseRecords.Last().WK_OIDN.Value
            && ((ModifyRoadNodeChange)x).Grensknoop == context.Change.DataSet.RoadNodeDbaseRecords.Last().GRENSKNOOP.Value.ToBooleanFromDbaseValue());
    }

    [Fact]
    public async Task WhenChangingTheRoadNodeIdAndGeometrySlightly_ThenReuseTheRoadNodeIdFromExtractFeature()
    {
        var (zipArchive, context) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                var dbaseRecord = builder.TestData.RoadSegment1StartNodeDbaseRecord;
                var shapeRecord = builder.TestData.RoadSegment1StartNodeShapeRecord;

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
        actualChanges.Should().Contain(x => x is ModifyRoadNodeChange
            && ((ModifyRoadNodeChange)x).RoadNodeId == new RoadNodeId(context.Extract.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value)
            && ((ModifyRoadNodeChange)x).Geometry == context.Change.TestData.RoadSegment1StartNodeShapeRecord.Geometry.ToRoadNodeGeometry());

        actualChanges.Should().Contain(x => x is ModifyRoadSegmentChange);
    }

    [Fact]
    public async Task IdsShouldBeUniqueAcrossChangeAndIntegrationData()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var integrationRoadNode = context.Integration.DataSet.RoadNodeDbaseRecords.First();

                builder.TestData.RoadSegment1StartNodeDbaseRecord.WK_OIDN.Value = integrationRoadNode.WK_OIDN.Value;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.RoadNodeIdentifierNotUniqueAcrossIntegrationAndChange), problem.Reason);
    }

    [Fact]
    public async Task GivenActualAndTemporarySchijnknopen_WhenSegmentIsChanged_ThenOnlyActualSchijnknoopIsRemoved()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithIntegration((builder, _) =>
            {
                builder.DataSet.Clear();
            })
            .WithExtract((builder, context) =>
            {
                ConfigureExtractScenario(builder);
            })
            .WithChange((builder, context) =>
            {
                ConfigureExtractScenario(builder);

                foreach (var roadNode in builder.DataSet.RoadNodeDbaseRecords)
                {
                    roadNode.TYPE.Value = 0;
                }

                builder.DataSet.RoadSegmentDbaseRecords[0].MORF.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentMorphologyV2.ByIdentifier[builder.DataSet.RoadSegmentDbaseRecords[0].MORF.Value]);
            })
            .Build();

        var changes = await TranslateSucceeds(zipArchive);
        changes.Should().Contain(x => x is ModifyRoadSegmentChange && ((ModifyRoadSegmentChange)x).RoadSegmentIdReference.RoadSegmentId == 1);
        changes.Should().Contain(x => x is RemoveRoadNodeChange && ((RemoveRoadNodeChange)x).RoadNodeId == 2);
        changes.Should().NotContain(x => x is ModifyRoadNodeChange && ((ModifyRoadNodeChange)x).RoadNodeId == 2);
        changes.Should().NotContain(x => x is RemoveRoadNodeChange && ((RemoveRoadNodeChange)x).RoadNodeId == 1_000_000_000);
        changes.Should().NotContain(x => x is ModifyRoadNodeChange && ((ModifyRoadNodeChange)x).RoadNodeId == 1_000_000_000);
        changes.Should().NotContain(x => x is AddRoadNodeChange && ((AddRoadNodeChange)x).TemporaryId == 1_000_000_000);

        void ConfigureExtractScenario(ExtractsZipArchiveExtractDataSetBuilder builder)
        {
            builder.DataSet.Clear();

            var start = new Point(650000, 650000).WithSrid(WellknownSrids.Lambert08);
            var actualSchijnknoop = new Point(650010, 650000).WithSrid(WellknownSrids.Lambert08);
            var tempSchijnknoop = new Point(650020, 650000).WithSrid(WellknownSrids.Lambert08);
            var end = new Point(650030, 650000).WithSrid(WellknownSrids.Lambert08);

            builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(start));
            builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
            {
                x.WK_OIDN.Value = 1;
                x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                x.GRENSKNOOP.Value = 0;
            }));

            builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(actualSchijnknoop));
            builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
            {
                x.WK_OIDN.Value = 2;
                x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                x.GRENSKNOOP.Value = 0;
            }));

            builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(tempSchijnknoop));
            builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
            {
                x.WK_OIDN.Value = 1_000_000_000;
                x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                x.GRENSKNOOP.Value = 0;
            }));

            builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(end));
            builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
            {
                x.WK_OIDN.Value = 4;
                x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                x.GRENSKNOOP.Value = 0;
            }));

            builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([start.Coordinate, actualSchijnknoop.Coordinate])));
            var roadSegmentDbaseRecord1 = builder.CreateRoadSegmentDbaseRecord(x =>
            {
                x.WS_TEMPID.Value = 1;
                x.WS_OIDN.Value = 1;
                x.VERHARDING.Value = RoadSegmentSurfaceTypeV2.Halfverhard;
            });
            builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord1);

            var roadSegmentDbaseRecord2 = roadSegmentDbaseRecord1.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
            roadSegmentDbaseRecord2.WS_TEMPID.Value = 2;
            roadSegmentDbaseRecord2.VERHARDING.Value = RoadSegmentSurfaceTypeV2.Onverhard;
            builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([actualSchijnknoop.Coordinate, tempSchijnknoop.Coordinate])));
            builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord2);

            var roadSegmentDbaseRecord3 = roadSegmentDbaseRecord1.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
            roadSegmentDbaseRecord3.WS_TEMPID.Value = 3;
            roadSegmentDbaseRecord3.VERHARDING.Value = RoadSegmentSurfaceTypeV2.Verhard;
            builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([tempSchijnknoop.Coordinate, end.Coordinate])));
            builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord3);

            builder.DataSet.TransactionZoneShapeRecords[0].Geometry = ((NetTopologySuite.Geometries.Polygon)start.Buffer(100)).ToMultiPolygon();
        }
    }

    [Fact]
    public async Task GivenActualAndTemporarySchijnknopen_WhenBothAreRemoved_ThenOnlyActualIsRemoved()
    {
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithIntegration((builder, _) =>
            {
                builder.DataSet.Clear();
            })
            .WithExtract((builder, _) =>
            {
                builder.DataSet.Clear();

                var start = new Point(650000, 650000).WithSrid(WellknownSrids.Lambert08);
                var actualSchijnknoop = new Point(650010, 650000).WithSrid(WellknownSrids.Lambert08);
                var tempSchijnknoop = new Point(650020, 650000).WithSrid(WellknownSrids.Lambert08);
                var end = new Point(650030, 650000).WithSrid(WellknownSrids.Lambert08);

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(start));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 1;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(actualSchijnknoop));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 2;
                    x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(tempSchijnknoop));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 1_000_000_000;
                    x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(end));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 4;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([start.Coordinate, actualSchijnknoop.Coordinate])));
                var roadSegmentDbaseRecord1 = builder.CreateRoadSegmentDbaseRecord(x =>
                {
                    x.WS_TEMPID.Value = 1;
                    x.WS_OIDN.Value = 1;
                    x.VERHARDING.Value = RoadSegmentSurfaceTypeV2.Halfverhard;
                });
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord1);

                var roadSegmentDbaseRecord2 = roadSegmentDbaseRecord1.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                roadSegmentDbaseRecord2.WS_TEMPID.Value = 2;
                roadSegmentDbaseRecord2.VERHARDING.Value = RoadSegmentSurfaceTypeV2.Onverhard;
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([actualSchijnknoop.Coordinate, tempSchijnknoop.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord2);

                var roadSegmentDbaseRecord3 = roadSegmentDbaseRecord1.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                roadSegmentDbaseRecord3.WS_TEMPID.Value = 3;
                roadSegmentDbaseRecord3.VERHARDING.Value = RoadSegmentSurfaceTypeV2.Verhard;
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([tempSchijnknoop.Coordinate, end.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord3);

                builder.DataSet.TransactionZoneShapeRecords[0].Geometry = ((NetTopologySuite.Geometries.Polygon)start.Buffer(100)).ToMultiPolygon();
            })
            .WithChange((builder, _) =>
            {
                builder.DataSet.Clear();

                // without schijnknopen and thus only 1 segment
                var start = new Point(650000, 650000).WithSrid(WellknownSrids.Lambert08);
                var end = new Point(650030, 650000).WithSrid(WellknownSrids.Lambert08);

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(start));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 1;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(end));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 4;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadSegmentShapeRecords.Clear();
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([start.Coordinate, end.Coordinate])));
                var roadSegmentDbaseRecord1 = builder.CreateRoadSegmentDbaseRecord(x =>
                {
                    x.WS_TEMPID.Value = 1;
                    x.WS_OIDN.Value = 1;
                });
                builder.DataSet.RoadSegmentDbaseRecords.Clear();
                builder.DataSet.RoadSegmentDbaseRecords.Add(roadSegmentDbaseRecord1);

                builder.DataSet.TransactionZoneShapeRecords[0].Geometry = ((NetTopologySuite.Geometries.Polygon)start.Buffer(100)).ToMultiPolygon();
            })
            .Build();

        var changes = await TranslateSucceeds(zipArchive);
        changes.Should().Contain(x => x is ModifyRoadSegmentChange && ((ModifyRoadSegmentChange)x).RoadSegmentIdReference.RoadSegmentId == 1);
        changes.Should().Contain(x => x is RemoveRoadNodeChange && ((RemoveRoadNodeChange)x).RoadNodeId == 2);
        changes.Should().NotContain(x => x is RemoveRoadNodeChange && ((RemoveRoadNodeChange)x).RoadNodeId == 1_000_000_000);
    }

    [Fact]
    public async Task WhenTempSchijnknoopBecomesStructural_ThenKeepAddAndRemoveConsumedRealNode()
    {
        // Arrange: extract with a real schijnknoop (node 2) and a temporary schijnknoop (node 1_000_000_000)
        // Change: same topology, but a new branch segment connects at the temp schijnknoop position,
        // making it a 3-way junction (structural/EchteKnoop). The real schijnknoop is consumed by unflattening.
        var zipArchive = new DomainV2ZipArchiveBuilder(fixture => fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd))
            .WithIntegration((builder, _) =>
            {
                builder.DataSet.Clear();
            })
            .WithExtract((builder, _) =>
            {
                builder.DataSet.Clear();

                var start = new Point(650000, 650000).WithSrid(WellknownSrids.Lambert08);
                var realSchijnknoop = new Point(650010, 650000).WithSrid(WellknownSrids.Lambert08);
                var tempSchijnknoop = new Point(650020, 650000).WithSrid(WellknownSrids.Lambert08);
                var end = new Point(650030, 650000).WithSrid(WellknownSrids.Lambert08);

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(start));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 1;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(realSchijnknoop));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 2;
                    x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(tempSchijnknoop));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 1_000_000_000;
                    x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(end));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 4;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                var seg1 = builder.CreateRoadSegmentDbaseRecord(x =>
                {
                    x.WS_TEMPID.Value = 1;
                    x.WS_OIDN.Value = 1;
                });
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([start.Coordinate, realSchijnknoop.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(seg1);

                var seg2 = seg1.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                seg2.WS_TEMPID.Value = 2;
                seg2.WS_OIDN.Value = 2;
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([realSchijnknoop.Coordinate, tempSchijnknoop.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(seg2);

                var seg3 = seg1.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                seg3.WS_TEMPID.Value = 3;
                seg3.WS_OIDN.Value = 3;
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([tempSchijnknoop.Coordinate, end.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(seg3);

                builder.DataSet.TransactionZoneShapeRecords[0].Geometry = ((NetTopologySuite.Geometries.Polygon)start.Buffer(100)).ToMultiPolygon();
            })
            .WithChange((builder, context) =>
            {
                builder.DataSet.Clear();

                var start = new Point(650000, 650000).WithSrid(WellknownSrids.Lambert08);
                var realSchijnknoop = new Point(650010, 650000).WithSrid(WellknownSrids.Lambert08);
                var tempSchijnknoop = new Point(650020, 650000).WithSrid(WellknownSrids.Lambert08);
                var end = new Point(650030, 650000).WithSrid(WellknownSrids.Lambert08);
                var branchEnd = new Point(650020, 650010).WithSrid(WellknownSrids.Lambert08);

                // Nodes: recreate the same four nodes from extract, plus a new node at branchEnd
                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(start));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 1;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(realSchijnknoop));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 2;
                    x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(tempSchijnknoop));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 1_000_000_000;
                    x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(end));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 4;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                // New node at branch end — causes temp schijnknoop to become a 3-way junction
                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(branchEnd));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                {
                    x.WK_OIDN.Value = 5;
                    x.TYPE.Value = RoadNodeTypeV2.Eindknoop;
                    x.GRENSKNOOP.Value = 0;
                }));

                // Segments: same 3 as extract, plus a new branch from the temp schijnknoop
                var baseRecord = context.Extract.DataSet.RoadSegmentDbaseRecords[0];

                var seg1 = baseRecord.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                seg1.WS_TEMPID.Value = 1;
                seg1.WS_OIDN.Value = 1;
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([start.Coordinate, realSchijnknoop.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(seg1);

                var seg2 = baseRecord.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                seg2.WS_TEMPID.Value = 2;
                seg2.WS_OIDN.Value = 2;
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([realSchijnknoop.Coordinate, tempSchijnknoop.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(seg2);

                var seg3 = baseRecord.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                seg3.WS_TEMPID.Value = 3;
                seg3.WS_OIDN.Value = 3;
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([tempSchijnknoop.Coordinate, end.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(seg3);

                // New branch segment — makes node 1_000_000_000 a 3-way structural junction
                var seg4 = baseRecord.Clone(new RecyclableMemoryStreamManager(), FileEncoding.UTF8);
                seg4.WS_TEMPID.Value = 4;
                seg4.WS_OIDN.Value = null;
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(new LineString([tempSchijnknoop.Coordinate, branchEnd.Coordinate])));
                builder.DataSet.RoadSegmentDbaseRecords.Add(seg4);

                builder.DataSet.TransactionZoneShapeRecords[0].Geometry = ((NetTopologySuite.Geometries.Polygon)start.Buffer(100)).ToMultiPolygon();
            })
            .Build();

        // Act
        var changes = await TranslateSucceeds(zipArchive);

        // Assert: the temp schijnknoop becomes structural (EchteKnoop), so its AddRoadNodeChange is kept
        changes.Should().Contain(x => x is AddRoadNodeChange && ((AddRoadNodeChange)x).TemporaryId == 1_000_000_000);
        // The real schijnknoop is consumed by unflattening (its two segments merge), so it gets a RemoveRoadNodeChange
        changes.Should().Contain(x => x is RemoveRoadNodeChange && ((RemoveRoadNodeChange)x).RoadNodeId == 2);
        changes.Should().NotContain(x => x is ModifyRoadNodeChange && ((ModifyRoadNodeChange)x).RoadNodeId == 2);
    }
}
