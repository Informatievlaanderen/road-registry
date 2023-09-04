namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using Be.Vlaanderen.Basisregisters.Shaperon;
using FeatureCompare;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;

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
                var lineString = GeometryTranslator.ToMultiLineString(builder.TestData.RoadSegment1ShapeRecord.Shape).GetSingleLineString();
                var endPointGeometry = new Point(lineString.Coordinates[1].X + 0.01, lineString.Coordinates[1].Y);
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(endPointGeometry.X, endPointGeometry.Y, lineString.Coordinates[1].M + 0.01)
                });
                builder.TestData.RoadSegment1ShapeRecord = lineString.ToShapeContent();
                builder.DataSet.RoadSegmentShapeRecords.RemoveAt(0);
                builder.DataSet.RoadSegmentShapeRecords.Insert(0, builder.TestData.RoadSegment1ShapeRecord);

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;

                builder.TestData.RoadNode2ShapeRecord = builder.CreateRoadNodeShapeRecord(endPointGeometry);
                builder.DataSet.RoadNodeShapeRecords.RemoveAt(1);
                builder.DataSet.RoadNodeShapeRecords.Insert(1, builder.TestData.RoadNode2ShapeRecord);
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(new ModifyRoadSegment(
                    new RecordNumber(1),
                    new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                    new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                    new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEER.Value),
                    RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                    RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORF.Value],
                    RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value],
                    RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.WEGCAT.Value],
                    RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value],
                    CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                    CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)
                )
                .WithGeometry(GeometryTranslator.ToMultiLineString(context.Change.TestData.RoadSegment1ShapeRecord.Shape))
                .WithLane(
                    new RoadSegmentLaneAttribute(
                        new AttributeId(context.Change.TestData.RoadSegment1LaneDbaseRecord.RS_OIDN.Value),
                        new RoadSegmentLaneCount(context.Change.TestData.RoadSegment1LaneDbaseRecord.AANTAL.Value),
                        RoadSegmentLaneDirection.ByIdentifier[context.Change.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value],
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value))
                    )
                )
                .WithWidth(
                    new RoadSegmentWidthAttribute(
                        new AttributeId(context.Change.TestData.RoadSegment1WidthDbaseRecord.WB_OIDN.Value),
                        new RoadSegmentWidth(context.Change.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value))
                    )
                )
                .WithSurface(
                    new RoadSegmentSurfaceAttribute(
                        new AttributeId(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.WV_OIDN.Value),
                        RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value],
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value))
                    )
                )));
        
        await TranslateReturnsExpectedResult(zipArchive, expected);
    }

    [Fact]
    public async Task ModifiedGeometryToMoreThanClusterTolerance()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var lineString = GeometryTranslator.ToMultiLineString(builder.TestData.RoadSegment1ShapeRecord.Shape).GetSingleLineString();
                var endPointGeometry = new Point(lineString.Coordinates[1].X + 0.06, lineString.Coordinates[1].Y);
                lineString = new LineString(new[]
                {
                    lineString.Coordinates[0],
                    new CoordinateM(endPointGeometry.X, endPointGeometry.Y, lineString.Coordinates[1].M + 0.06)
                });
                builder.TestData.RoadSegment1ShapeRecord = lineString.ToShapeContent();
                builder.DataSet.RoadSegmentShapeRecords.RemoveAt(0);
                builder.DataSet.RoadSegmentShapeRecords.Insert(0, builder.TestData.RoadSegment1ShapeRecord);

                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;
                builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Shape.MeasureRange.Max;

                builder.TestData.RoadNode2ShapeRecord = builder.CreateRoadNodeShapeRecord(endPointGeometry);
                builder.DataSet.RoadNodeShapeRecords.RemoveAt(1);
                builder.DataSet.RoadNodeShapeRecords.Insert(1, builder.TestData.RoadNode2ShapeRecord);
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(new RemoveRoadNode(
                    new RecordNumber(2),
                    new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value)
                ))
                .AppendChange(new AddRoadNode(
                    new RecordNumber(2),
                    new RoadNodeId(context.Change.TestData.RoadNode2DbaseRecord.WK_OIDN.Value),
                        RoadNodeType.ByIdentifier[context.Change.TestData.RoadNode2DbaseRecord.TYPE.Value]
                    )
                    .WithGeometry(GeometryTranslator.ToPoint(context.Change.TestData.RoadNode2ShapeRecord.Shape))
                )
                .AppendChange(new ModifyRoadSegment(
                    new RecordNumber(1),
                    new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value),
                    new RoadNodeId(context.Change.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value),
                    new OrganizationId(context.Change.TestData.RoadSegment1DbaseRecord.BEHEER.Value),
                    RoadSegmentGeometryDrawMethod.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.METHODE.Value],
                    RoadSegmentMorphology.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.MORF.Value],
                    RoadSegmentStatus.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.STATUS.Value],
                    RoadSegmentCategory.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.WEGCAT.Value],
                    RoadSegmentAccessRestriction.ByIdentifier[context.Change.TestData.RoadSegment1DbaseRecord.TGBEP.Value],
                    CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.LSTRNMID.Value),
                    CrabStreetnameId.FromValue(context.Change.TestData.RoadSegment1DbaseRecord.RSTRNMID.Value)
                )
                .WithGeometry(GeometryTranslator.ToMultiLineString(context.Change.TestData.RoadSegment1ShapeRecord.Shape))
                .WithLane(
                    new RoadSegmentLaneAttribute(
                        new AttributeId(context.Change.TestData.RoadSegment1LaneDbaseRecord.RS_OIDN.Value),
                        new RoadSegmentLaneCount(context.Change.TestData.RoadSegment1LaneDbaseRecord.AANTAL.Value),
                        RoadSegmentLaneDirection.ByIdentifier[context.Change.TestData.RoadSegment1LaneDbaseRecord.RICHTING.Value],
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value))
                    )
                )
                .WithWidth(
                    new RoadSegmentWidthAttribute(
                        new AttributeId(context.Change.TestData.RoadSegment1WidthDbaseRecord.WB_OIDN.Value),
                        new RoadSegmentWidth(context.Change.TestData.RoadSegment1WidthDbaseRecord.BREEDTE.Value),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value))
                    )
                )
                .WithSurface(
                    new RoadSegmentSurfaceAttribute(
                        new AttributeId(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.WV_OIDN.Value),
                        RoadSegmentSurfaceType.ByIdentifier[context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TYPE.Value],
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(context.Change.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value))
                    )
                )));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }
}
