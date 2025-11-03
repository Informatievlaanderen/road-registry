//TODO-pr uncomment
// namespace RoadRegistry.Wms.Projections.Tests.Projections;
//
// using AutoFixture;
// using BackOffice;
// using BackOffice.Messages;
// using RoadRegistry.Tests.BackOffice;
// using RoadRegistry.Tests.Framework.Projections;
// using Schema;
// using Wms.Projections;
//
// public class RoadSegmentEuropeanRoadAttributeRecordProjectionTests
// {
//     private readonly Fixture _fixture;
//
//     public RoadSegmentEuropeanRoadAttributeRecordProjectionTests()
//     {
//         _fixture = new Fixture();
//         _fixture.CustomizeArchiveId();
//         _fixture.CustomizeAttributeId();
//         _fixture.CustomizeRoadSegmentId();
//         _fixture.CustomizeRoadNodeId();
//         _fixture.CustomizeOrganizationId();
//         _fixture.CustomizeOrganizationName();
//         _fixture.CustomizePolylineM();
//         _fixture.CustomizeEuropeanRoadNumber();
//         _fixture.CustomizeNationalRoadNumber();
//         _fixture.CustomizeNumberedRoadNumber();
//         _fixture.CustomizeRoadSegmentNumberedRoadDirection();
//         _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
//         _fixture.CustomizeRoadSegmentLaneCount();
//         _fixture.CustomizeRoadSegmentLaneDirection();
//         _fixture.CustomizeRoadSegmentWidth();
//         _fixture.CustomizeRoadSegmentSurfaceType();
//         _fixture.CustomizeRoadSegmentGeometryDrawMethod();
//         _fixture.CustomizeRoadSegmentMorphology();
//         _fixture.CustomizeRoadSegmentStatus();
//         _fixture.CustomizeRoadSegmentCategory();
//         _fixture.CustomizeRoadSegmentAccessRestriction();
//         _fixture.CustomizeRoadSegmentGeometryVersion();
//
//         _fixture.CustomizeImportedRoadSegment();
//         _fixture.CustomizeImportedRoadSegmentEuropeanRoadAttributes();
//         _fixture.CustomizeImportedRoadSegmentNationalRoadAttributes();
//         _fixture.CustomizeImportedRoadSegmentNumberedRoadAttributes();
//         _fixture.CustomizeImportedRoadSegmentLaneAttributes();
//         _fixture.CustomizeImportedRoadSegmentWidthAttributes();
//         _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
//         _fixture.CustomizeImportedRoadSegmentSideAttributes();
//         _fixture.CustomizeOriginProperties();
//
//         _fixture.CustomizeRoadNetworkChangesAccepted();
//
//         _fixture.CustomizeRoadSegmentAddedToEuropeanRoad();
//         _fixture.CustomizeRoadSegmentRemovedFromEuropeanRoad();
//     }
//
//     [Fact]
//     public Task When_adding_road_nodes()
//     {
//         var message = _fixture
//             .Create<RoadNetworkChangesAccepted>()
//             .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToEuropeanRoad>());
//
//         var expectedRecords = Array.ConvertAll(message.Changes, change =>
//         {
//             var europeanRoad = change.RoadSegmentAddedToEuropeanRoad;
//
//             return (object)new RoadSegmentEuropeanRoadAttributeRecord
//             {
//                 EU_OIDN = europeanRoad.AttributeId,
//                 WS_OIDN = europeanRoad.SegmentId,
//                 EUNUMMER = europeanRoad.Number,
//                 BEGINTIJD = LocalDateTimeTranslator.TranslateFromWhen(message.When),
//                 BEGINORG = message.OrganizationId,
//                 LBLBGNORG = message.Organization
//             };
//         });
//
//         return new RoadSegmentEuropeanRoadAttributeRecordProjection()
//             .Scenario()
//             .Given(message)
//             .Expect(expectedRecords);
//     }
//
//     [Fact]
//     public Task When_importing_a_road_node_without_european_road_links()
//     {
//         var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
//         importedRoadSegment.PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttribute>();
//
//         return new RoadSegmentEuropeanRoadAttributeRecordProjection()
//             .Scenario()
//             .Given(importedRoadSegment)
//             .Expect();
//     }
//
//     [Fact]
//     public Task When_importing_road_nodes()
//     {
//         var random = new Random();
//         var data = _fixture
//             .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
//             .Select(segment =>
//             {
//                 segment.PartOfEuropeanRoads = _fixture
//                     .CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>(random.Next(1, 10))
//                     .ToArray();
//
//                 var expected = segment
//                     .PartOfEuropeanRoads
//                     .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
//                     {
//                         EU_OIDN = europeanRoad.AttributeId,
//                         WS_OIDN = segment.Id,
//                         EUNUMMER = europeanRoad.Number,
//                         BEGINTIJD = europeanRoad.Origin.Since,
//                         BEGINORG = europeanRoad.Origin.OrganizationId,
//                         LBLBGNORG = europeanRoad.Origin.Organization
//                     });
//
//                 return new
//                 {
//                     importedRoadSegment = segment,
//                     expected
//                 };
//             }).ToList();
//
//         return new RoadSegmentEuropeanRoadAttributeRecordProjection()
//             .Scenario()
//             .Given(data.Select(d => d.importedRoadSegment))
//             .Expect(data
//                 .SelectMany(d => d.expected)
//                 .Cast<object>()
//                 .ToArray()
//             );
//     }
//
//     [Fact]
//     public Task When_removing_road_segments()
//     {
//         var roadSegmentAddedToEuropeanRoad = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();
//         var anotherRoadSegmentAddedToEuropeanRoad = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();
//
//         var acceptedRoadSegmentsAdded = _fixture
//             .Create<RoadNetworkChangesAccepted>()
//             .WithAcceptedChanges(
//                 roadSegmentAddedToEuropeanRoad,
//                 anotherRoadSegmentAddedToEuropeanRoad);
//
//         var acceptedRoadSegmentRemoved = _fixture
//             .Create<RoadNetworkChangesAccepted>()
//             .WithAcceptedChanges(new RoadSegmentRemoved
//             {
//                 Id = anotherRoadSegmentAddedToEuropeanRoad.SegmentId
//             });
//
//         return new RoadSegmentEuropeanRoadAttributeRecordProjection()
//             .Scenario()
//             .Given(acceptedRoadSegmentsAdded, acceptedRoadSegmentRemoved)
//             .Expect(new RoadSegmentEuropeanRoadAttributeRecord
//             {
//                 EU_OIDN = roadSegmentAddedToEuropeanRoad.AttributeId,
//                 WS_OIDN = roadSegmentAddedToEuropeanRoad.SegmentId,
//                 EUNUMMER = roadSegmentAddedToEuropeanRoad.Number,
//                 BEGINTIJD = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When),
//                 BEGINORG = acceptedRoadSegmentsAdded.OrganizationId,
//                 LBLBGNORG = acceptedRoadSegmentsAdded.Organization
//             });
//     }
//
//     [Fact]
//     public Task When_removing_road_segments_from_european_roads()
//     {
//         _fixture.Freeze<AttributeId>();
//
//         var acceptedRoadSegmentAdded = _fixture
//             .Create<RoadNetworkChangesAccepted>()
//             .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToEuropeanRoad>());
//
//         var acceptedRoadSegmentRemoved = _fixture
//             .Create<RoadNetworkChangesAccepted>()
//             .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromEuropeanRoad>());
//
//         return new RoadSegmentEuropeanRoadAttributeRecordProjection()
//             .Scenario()
//             .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
//             .ExpectNone();
//     }
// }
