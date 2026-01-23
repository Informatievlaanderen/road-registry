//TODO-pr uncomment bij implementatie upload
// namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V3.Scenarios;
//
// using GradeSeparatedJunction.Changes;
// using Microsoft.Extensions.Logging;
// using NetTopologySuite.Geometries;
// using RoadRegistry.Extensions;
// using RoadRegistry.Extracts.FeatureCompare.DomainV2;
// using RoadRegistry.Extracts.Uploads;
// using RoadRegistry.Tests.BackOffice;
// using RoadRegistry.Tests.BackOffice.Extracts.V2;
// using Xunit.Abstractions;
// using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;
//
// public class GradeSeparatedJunctionScenarios : FeatureCompareTranslatorScenariosBase
// {
//     public GradeSeparatedJunctionScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
//         : base(testOutputHelper, logger)
//     {
//     }
//
//     [Fact]
//     public async Task RemovedRoadSegmentShouldGiveProblem()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange((builder, context) =>
//             {
//                 builder.DataSet.RoadSegmentDbaseRecords = new[] { builder.TestData.RoadSegment1DbaseRecord }.ToList();
//                 builder.DataSet.RoadSegmentShapeRecords = new[] { builder.TestData.RoadSegment1ShapeRecord }.ToList();
//
//                 builder.DataSet.SurfaceDbaseRecords = new[] { builder.TestData.RoadSegment1SurfaceDbaseRecord }.ToList();
//             })
//             .BuildWithResult(_ => TranslatedChanges.Empty);
//
//         var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
//         Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange));
//     }
//
//     [Fact]
//     public async Task EqualLowerAndUpperShouldGiveProblem()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange((builder, context) =>
//             {
//                 builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value;
//             })
//             .BuildWithResult(_ => TranslatedChanges.Empty);
//
//         var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
//         Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegment));
//     }
//
//     [Fact]
//     public async Task UnknownRoadSegmentShouldGiveProblem()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange((builder, context) =>
//             {
//                 var fixture = context.Fixture;
//
//                 builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value = fixture.CreateWhichIsDifferentThan(
//                     builder.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value,
//                     builder.TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value);
//
//                 builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_TEMPID.Value = fixture.CreateWhichIsDifferentThan(
//                     builder.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value,
//                     builder.TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value,
//                     builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_TEMPID.Value);
//             })
//             .BuildWithResult(_ => TranslatedChanges.Empty);
//
//         var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
//         Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange));
//         Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.UpperRoadSegmentIdOutOfRange));
//     }
//
//     [Fact]
//     public async Task WhenIntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunction_ThenShouldGiveProblem()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange(ConfigureIntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunction)
//             .BuildWithResult(_ => TranslatedChanges.Empty);
//
//         var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
//         Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionMissing));
//     }
//
//     [Fact]
//     public async Task WhenIntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunction_WithChangedRoadSegmentsWithNullGeometry_ThenShouldOnlyReturnProblem()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange((builder, context) =>
//             {
//                 // trigger partial update
//                 builder.TestData.RoadSegment1DbaseRecord.MORF.Value = context.Fixture.CreateWhichIsDifferentThan(RoadSegmentMorphology.ByIdentifier[builder.TestData.RoadSegment1DbaseRecord.MORF.Value]).Translation.Identifier;
//
//                 // add segment which intersects with existing segment to trigger checking missing gradeseparatedjunctions
//                 var newNode1Dbase = builder.CreateRoadNodeDbaseRecord();
//                 var newNode1Shape = builder.CreateRoadNodeShapeRecord();
//                 var newNode2Dbase = builder.CreateRoadNodeDbaseRecord();
//                 var newNode2Shape = builder.CreateRoadNodeShapeRecord();
//                 var newSegment1Dbase = builder.CreateRoadSegmentDbaseRecord();
//                 newSegment1Dbase.B_WK_OIDN.Value = newNode1Dbase.WK_OIDN.Value;
//                 newSegment1Dbase.E_WK_OIDN.Value = newNode2Dbase.WK_OIDN.Value;
//                 var newSegment1Shape = builder.CreateRoadSegmentShapeRecord();
//                 var intersection = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString().Centroid;
//                 var newSegment1GeometryCrossingSegment1 = new LineString([
//                     new (intersection.X, intersection.Y - 1),
//                     new (intersection.X, intersection.Y + 5)
//                 ]);
//                 newSegment1Shape.Geometry = newSegment1GeometryCrossingSegment1.ToMultiLineString();
//                 var newSegment1Surface = builder.CreateRoadSegmentSurfaceDbaseRecord();
//                 newSegment1Surface.WS_OIDN.Value = newSegment1Dbase.WS_OIDN.Value;
//                 newSegment1Surface.VANPOS.Value = 0;
//                 newSegment1Surface.TOTPOS.Value = newSegment1Shape.Geometry.Length;
//
//                 builder.DataSet.RoadNodeDbaseRecords.Add(newNode1Dbase);
//                 builder.DataSet.RoadNodeDbaseRecords.Add(newNode2Dbase);
//                 builder.DataSet.RoadNodeShapeRecords.Add(newNode1Shape);
//                 builder.DataSet.RoadNodeShapeRecords.Add(newNode2Shape);
//                 builder.DataSet.RoadSegmentDbaseRecords.Add(newSegment1Dbase);
//                 builder.DataSet.RoadSegmentShapeRecords.Add(newSegment1Shape);
//                 builder.DataSet.SurfaceDbaseRecords.Add(newSegment1Surface);
//             })
//             .BuildWithResult(_ => TranslatedChanges.Empty);
//
//         var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
//         Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.GradeSeparatedJunctionMissing));
//     }
//
//     private static void ConfigureIntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunction(ExtractsZipArchiveExtractDataSetBuilder builder, ExtractsZipArchiveChangeDataSetBuilderContext context)
//     {
//         var roadSegment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
//
//         var intersection = roadSegment1Geometry.Centroid;
//
//         var roadSegment2Geometry = new LineString([
//             new (intersection.X, intersection.Y - 1),
//             new (intersection.X, intersection.Y + 5)
//         ]);
//         builder.TestData.RoadSegment2ShapeRecord.Geometry = roadSegment2Geometry.ToMultiLineString();
//
//         builder.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
//
//         builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
//     }
//
//     [Fact]
//     public async Task IntersectingOutlinedRoadSegmentsWithoutGradeSeparatedJunctionShouldNotGiveProblem()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder(fixture =>
//             {
//                 fixture.CustomizeRoadSegmentOutlineLaneCount();
//                 fixture.CustomizeRoadSegmentOutlineMorphology();
//                 fixture.CustomizeRoadSegmentOutlineStatus();
//                 fixture.CustomizeRoadSegmentOutlineWidth();
//             })
//             .WithChange((builder, context) =>
//             {
//                 ConfigureIntersectingMeasuredRoadSegmentsWithoutGradeSeparatedJunction(builder, context);
//
//                 builder.TestData.RoadSegment1DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
//                 builder.TestData.RoadSegment1DbaseRecord.B_WK_OIDN.Value = 0;
//                 builder.TestData.RoadSegment1DbaseRecord.E_WK_OIDN.Value = 0;
//                 builder.TestData.RoadSegment2DbaseRecord.METHODE.Value = RoadSegmentGeometryDrawMethod.Outlined.Translation.Identifier;
//                 builder.TestData.RoadSegment2DbaseRecord.B_WK_OIDN.Value = 0;
//                 builder.TestData.RoadSegment2DbaseRecord.E_WK_OIDN.Value = 0;
//             })
//             .BuildWithResult(_ => TranslatedChanges.Empty);
//
//         await TranslateSucceeds(zipArchive);
//     }
//
//     [Fact]
//     public async Task IntersectingRoadSegmentsAtTheirStartOrEndPointsWithoutGradeSeparatedJunctionShouldNotGiveProblem()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange((builder, context) =>
//             {
//                 var roadSegment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
//
//                 var intersection = roadSegment1Geometry.StartPoint;
//
//                 var roadSegment2Geometry = new LineString([
//                     new (intersection.X, intersection.Y),
//                     new (intersection.X, intersection.Y + 5)
//                 ]);
//                 builder.TestData.RoadSegment2ShapeRecord.Geometry = roadSegment2Geometry.ToMultiLineString();
//
//                 builder.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
//
//                 builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
//             })
//             .BuildWithResult(_ => TranslatedChanges.Empty);
//
//         await TranslateSucceeds(zipArchive);
//     }
//
//     [Fact]
//     public async Task IntersectingRoadSegmentsInA_TShape_WithoutGradeSeparatedJunctionShouldNotGiveProblem()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange((builder, context) =>
//             {
//                 var roadSegment1Geometry = builder.TestData.RoadSegment1ShapeRecord.Geometry.GetSingleLineString();
//
//                 var intersection = roadSegment1Geometry.Centroid;
//
//                 var roadSegment2Geometry = new LineString([
//                     new (intersection.X, intersection.Y),
//                     new (intersection.X, intersection.Y + 5)
//                 ]);
//                 builder.TestData.RoadSegment2ShapeRecord.Geometry = roadSegment2Geometry.ToMultiLineString();
//
//                 builder.TestData.RoadSegment2SurfaceDbaseRecord.TOTPOS.Value = roadSegment2Geometry.Length;
//
//                 builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
//             })
//             .BuildWithResult(_ => TranslatedChanges.Empty);
//
//         await TranslateSucceeds(zipArchive);
//     }
//
//     [Fact]
//     public async Task Updated_DifferentId()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange((builder, context) =>
//             {
//                 var fixture = context.Fixture;
//
//                 var gradeSeparatedJunctionDbaseRecord2 = builder.CreateGradeSeparatedJunctionDbaseRecord();
//                 gradeSeparatedJunctionDbaseRecord2.BO_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value;
//                 gradeSeparatedJunctionDbaseRecord2.ON_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value;
//                 gradeSeparatedJunctionDbaseRecord2.OK_OIDN.Value = fixture.CreateWhichIsDifferentThan(new GradeSeparatedJunctionId(builder.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value));
//                 gradeSeparatedJunctionDbaseRecord2.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
//                 builder.DataSet.GradeSeparatedJunctionDbaseRecords = new[] { gradeSeparatedJunctionDbaseRecord2 }.ToList();
//             })
//             .BuildWithResult(context =>
//             {
//                 var gradeSeparatedJunctionDbaseRecord2 = context.Change.DataSet.GradeSeparatedJunctionDbaseRecords.Single();
//
//                 return TranslatedChanges.Empty
//                     .AppendChange(
//                         new AddGradeSeparatedJunctionChange
//                         {
//                             TemporaryId = new GradeSeparatedJunctionId(gradeSeparatedJunctionDbaseRecord2.OK_OIDN.Value),
//                             Type = GradeSeparatedJunctionType.ByIdentifier[gradeSeparatedJunctionDbaseRecord2.TYPE.Value],
//                             UpperRoadSegmentId = new RoadSegmentId(gradeSeparatedJunctionDbaseRecord2.BO_WS_OIDN.Value),
//                             LowerRoadSegmentId = new RoadSegmentId(gradeSeparatedJunctionDbaseRecord2.ON_WS_OIDN.Value)
//                         }
//                     )
//                     .AppendChange(
//                         new RemoveGradeSeparatedJunctionChange
//                         {
//                             GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Extract.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
//                         }
//                     );
//             });
//
//         await TranslateReturnsExpectedResult(zipArchive, expected);
//     }
//
//     [Fact]
//     public async Task Updated_SameId()
//     {
//         var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
//             .WithChange((builder, context) =>
//             {
//                 var fixture = context.Fixture;
//
//                 builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value = fixture.CreateWhichIsDifferentThan(GradeSeparatedJunctionType.ByIdentifier[builder.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value]).Translation.Identifier;
//             })
//             .BuildWithResult(context =>
//             {
//                 return TranslatedChanges.Empty
//                     .AppendChange(
//                         new AddGradeSeparatedJunctionChange
//                         {
//                             TemporaryId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value),
//                             Type = GradeSeparatedJunctionType.ByIdentifier[context.Change.TestData.GradeSeparatedJunctionDbaseRecord.TYPE.Value],
//                             UpperRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
//                             LowerRoadSegmentId = new RoadSegmentId(context.Change.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value)
//                         }
//                     )
//                     .AppendChange(
//                         new RemoveGradeSeparatedJunctionChange
//                         {
//                             GradeSeparatedJunctionId = new GradeSeparatedJunctionId(context.Change.TestData.GradeSeparatedJunctionDbaseRecord.OK_OIDN.Value)
//                         }
//                     );
//             });
//
//         await TranslateReturnsExpectedResult(zipArchive, expected);
//     }
//
//     [Fact]
//     public async Task RemovingDuplicateRecordsShouldReturnExpectedResult()
//     {
//         var zipArchiveBuilder = new DomainV2ZipArchiveBuilder();
//
//         var duplicateGradeSeparatedJunction = zipArchiveBuilder.Records.CreateGradeSeparatedJunctionDbaseRecord();
//
//         var (zipArchive, expected) = zipArchiveBuilder
//             .WithExtract((builder, context) =>
//             {
//                 duplicateGradeSeparatedJunction.BO_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.BO_WS_OIDN.Value;
//                 duplicateGradeSeparatedJunction.ON_WS_OIDN.Value = builder.TestData.GradeSeparatedJunctionDbaseRecord.ON_WS_OIDN.Value;
//
//                 builder.DataSet.GradeSeparatedJunctionDbaseRecords.Add(duplicateGradeSeparatedJunction);
//             })
//             .BuildWithResult(context =>
//             {
//                 return TranslatedChanges.Empty
//                     .AppendChange(
//                         new RemoveGradeSeparatedJunctionChange
//                         {
//                             GradeSeparatedJunctionId = new GradeSeparatedJunctionId(duplicateGradeSeparatedJunction.OK_OIDN.Value)
//                         }
//                     );
//             });
//
//         await TranslateReturnsExpectedResult(zipArchive, expected);
//     }
// }
