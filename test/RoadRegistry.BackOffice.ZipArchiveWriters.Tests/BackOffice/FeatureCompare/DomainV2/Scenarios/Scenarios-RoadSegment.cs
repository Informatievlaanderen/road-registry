namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios;

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using Xunit.Abstractions;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;

public class RoadSegmentScenarios : FeatureCompareTranslatorScenariosBase
{
    public RoadSegmentScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    // Two delivered road segments claim the same WS_OIDN. The one overlapping the extract road segment most keeps the
    // id; the other one becomes a new road segment. A road segment that keeps its id is compared to the extract and
    // only carries what changed, so the new one has to be handed its own attributes in full - including whatever it
    // has in common with the extract road segment it no longer is.
    [Fact]
    public async Task WhenTwoRoadSegmentsReUseTheSameExtractRoadSegmentId_ThenTheNewOneCarriesAllItsAttributes()
    {
        var startPoint = new Point(602000, 602000).WithSrid(WellknownSrids.Lambert08);
        var endPoint = new Point(602100, 602000).WithSrid(WellknownSrids.Lambert08);

        var (zipArchive, context) = new DomainV2ZipArchiveBuilder(fixture => { fixture.Freeze(RoadSegmentStatusV2.Gerealiseerd); })
            .WithExtract((builder, _) =>
            {
                builder.TestData.RoadSegment1ShapeRecord.Geometry = new LineString([
                    startPoint.Coordinate,
                    endPoint.Coordinate
                ]).WithSrid(WellknownSrids.Lambert08).ToMultiLineString();

                builder.TestData.RoadSegment1StartNodeShapeRecord.Geometry = startPoint;
                builder.TestData.RoadSegment1EndNodeShapeRecord.Geometry = endPoint;
            })
            .WithChange((builder, _) =>
            {
                var srid = builder.TestData.RoadSegment1ShapeRecord.Geometry.SRID;

                // Road segment 1 keeps every attribute it has in the extract; only its geometry moves, so that it ends up
                // overlapping the extract road segment less than the other one does.
                builder.TestData.RoadSegment1ShapeRecord.Geometry = new LineString([
                    new Coordinate(startPoint.X + 5, startPoint.Y - 0.5),
                    new Coordinate(endPoint.X - 5, endPoint.Y - 0.5)
                ]).WithSrid(srid).ToMultiLineString();

                var otherGeometry = new LineString([
                    new Coordinate(startPoint.X - 5, startPoint.Y + 0.5),
                    new Coordinate(endPoint.X + 5, endPoint.Y + 0.5)
                ]).WithSrid(srid);
                builder.DataSet.RoadSegmentShapeRecords.Add(builder.CreateRoadSegmentShapeRecord(otherGeometry));
                builder.DataSet.RoadSegmentDbaseRecords.Add(builder.CreateRoadSegmentDbaseRecord(record =>
                    record.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value));

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(otherGeometry.StartPoint));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());

                builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord(otherGeometry.EndPoint));
                builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord());
            })
            .BuildWithContext();

        var changes = await TranslateSucceeds(zipArchive);

        var addRoadSegment = changes.OfType<AddRoadSegmentChange>().Single();
        addRoadSegment.RoadSegmentIdReference.TempIds!.Single().Should().Be(new RoadSegmentTempId(context.Change.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value));
        addRoadSegment.GeometryDrawMethod.Should().NotBeNull();
        addRoadSegment.Status.Should().NotBeNull();
        addRoadSegment.AccessRestriction.Should().NotBeNull();
        addRoadSegment.Category.Should().NotBeNull();
        addRoadSegment.MaintenanceAuthorityId.Should().NotBeNull();
        addRoadSegment.Morphology.Should().NotBeNull();
        addRoadSegment.StreetNameId.Should().NotBeNull();
        addRoadSegment.SurfaceType.Should().NotBeNull();
        addRoadSegment.CarTrafficDirection.Should().NotBeNull();
        addRoadSegment.BikeTrafficDirection.Should().NotBeNull();
        addRoadSegment.PedestrianTrafficDirection.Should().NotBeNull();
    }
}
