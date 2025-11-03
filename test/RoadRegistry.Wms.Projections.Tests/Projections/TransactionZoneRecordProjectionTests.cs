//TODO-pr uncomment
// namespace RoadRegistry.Wms.Projections.Tests.Projections;
//
// using AutoFixture;
// using BackOffice;
// using BackOffice.Messages;
// using Be.Vlaanderen.Basisregisters.Shaperon;
// using Microsoft.Extensions.Logging.Abstractions;
// using NetTopologySuite.Geometries;
// using NetTopologySuite.IO;
// using RoadRegistry.Tests.BackOffice;
// using RoadRegistry.Tests.Framework.Projections;
// using Schema;
// using Wms.Projections;
//
// public class TransactionZoneRecordProjectionTests
// {
//     private readonly Fixture _fixture;
//
//     public TransactionZoneRecordProjectionTests()
//     {
//         _fixture = new Fixture();
//         _fixture.CustomizeArchiveId();
//         _fixture.CustomizeExternalExtractRequestId();
//         _fixture.CustomizeRoadNetworkExtractGotRequested();
//         _fixture.CustomizeRoadNetworkExtractGotRequestedV2();
//         _fixture.CustomizeRoadNetworkExtractClosed();
//     }
//
//     [Fact]
//     public Task When_extract_got_requested()
//     {
//         var extract1 = _fixture.Create<RoadNetworkExtractGotRequested>();
//         extract1.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))"
//         };
//         var extract2 = _fixture.Create<RoadNetworkExtractGotRequested>();
//         extract2.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "POLYGON((5 5, 5 15, 15 15, 15 5, 5 5))"
//         };
//         var events = new[] { extract1, extract2 };
//
//         var expected = new List<object>();
//         expected.AddRange(events
//             .Select(requested => new TransactionZoneRecord
//             {
//                 DownloadId = requested.DownloadId,
//                 Description = requested.Description,
//                 Contour = (Geometry)GeometryTranslator.Translate(requested.Contour)
//             }));
//
//         var geometry = new WKTReader(WellKnownGeometryFactories.Default).Read("POLYGON((10 10, 10 5, 5 5, 5 10, 10 10))");
//         geometry.SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();
//
//         expected.Add(new OverlappingTransactionZonesRecord
//         {
//             DownloadId1 = extract1.DownloadId,
//             DownloadId2 = extract2.DownloadId,
//             Description1 = extract1.Description,
//             Description2 = extract2.Description,
//             Contour = geometry
//         });
//
//         return new TransactionZoneRecordProjection(new NullLoggerFactory())
//             .Scenario()
//             .Given(events.Cast<object>())
//             .Expect(expected);
//     }
//
//     [Fact]
//     public Task When_extract_got_requested_v2()
//     {
//         var extract1 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
//         extract1.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))"
//         };
//         var extract2 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
//         extract2.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "POLYGON((5 5, 5 15, 15 15, 15 5, 5 5))"
//         };
//         var events = new[] { extract1, extract2 };
//
//         var expected = new List<object>();
//         expected.AddRange(events
//             .Select(requested => new TransactionZoneRecord
//             {
//                 DownloadId = requested.DownloadId,
//                 Description = requested.Description,
//                 Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
//             }));
//
//         var geometry = new WKTReader(WellKnownGeometryFactories.Default).Read("POLYGON((10 10, 10 5, 5 5, 5 10, 10 10))");
//         geometry.SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32();
//
//         expected.Add(new OverlappingTransactionZonesRecord
//         {
//             DownloadId1 = extract1.DownloadId,
//             DownloadId2 = extract2.DownloadId,
//             Description1 = extract1.Description,
//             Description2 = extract2.Description,
//             Contour = geometry
//         });
//
//         return new TransactionZoneRecordProjection(new NullLoggerFactory())
//             .Scenario()
//             .Given(events.Cast<object>())
//             .Expect(expected);
//     }
//
//     [Fact]
//     public Task When_extract_closed()
//     {
//         var extract1 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
//         extract1.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))"
//         };
//         var extract2 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
//         extract2.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "POLYGON((5 5, 5 15, 15 15, 15 5, 5 5))"
//         };
//
//         var extract1Closed = new RoadNetworkExtractClosed
//         {
//             RequestId = extract1.RequestId,
//             ExternalRequestId = extract1.ExternalRequestId,
//             DownloadIds = [extract1.DownloadId.ToString("N")],
//             DateRequested = DateTime.UtcNow,
//             Reason = _fixture.Create<RoadNetworkExtractCloseReason>(),
//             When = extract1.When
//         };
//
//         var expectedContour = (Geometry)GeometryTranslator.Translate(extract2.Contour);
//         var expected = new List<object>
//         {
//             new TransactionZoneRecord
//             {
//                 DownloadId = extract2.DownloadId,
//                 Description = extract2.Description,
//                 Contour = expectedContour
//             }
//         };
//
//         return new TransactionZoneRecordProjection(new NullLoggerFactory())
//             .Scenario()
//             .Given(new[] { extract1, extract2 }.Cast<object>())
//             .Given(extract1Closed)
//             .Expect(expected);
//     }
//
//     [Fact]
//     public Task When_extract_upload_changes_got_accepted()
//     {
//         var extract1 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
//         extract1.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))"
//         };
//         var extract2 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
//         extract2.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "POLYGON((5 5, 5 15, 15 15, 15 5, 5 5))"
//         };
//
//         var extract1Accepted = new RoadNetworkChangesAccepted
//         {
//             RequestId = extract1.RequestId,
//             DownloadId = extract1.DownloadId,
//             When = extract1.When
//         };
//
//         var expectedContour = (Geometry)GeometryTranslator.Translate(extract2.Contour);
//         var expected = new List<object>
//         {
//             new TransactionZoneRecord
//             {
//                 DownloadId = extract2.DownloadId,
//                 Description = extract2.Description,
//                 Contour = expectedContour
//             }
//         };
//
//         return new TransactionZoneRecordProjection(new NullLoggerFactory())
//             .Scenario()
//             .Given(new[] { extract1, extract2 }.Cast<object>())
//             .Given(extract1Accepted)
//             .Expect(expected);
//     }
//
//     [Fact]
//     public Task WhenOverlapHasAreaOfExtremelyCloseToZero_ThenNoOverlap()
//     {
//         var extract1 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
//         extract1.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "MULTIPOLYGON (((103187.99730784446 151745.91685013473, 103408.20516492426 163826.64667826146, 104139.7936558798 164135.10030906647, 104351.46074587852 163288.43194905668, 107251.29987890273 164113.93360006064, 108711.80279989541 160769.5935780555, 111272.97458890826 160684.92674204707, 112437.14358391613 160007.59205403924, 113918.81321392953 160727.26016004384, 115993.15069593489 160727.26016004384, 117305.48665393144 159266.75723904371, 117241.98652693629 156620.91861402988, 118850.65641094744 154631.24796802551, 122935.83124795556 153848.07973501831, 124544.50113195926 155012.24873002619, 127994.67469897866 155245.08252903074, 128672.00938697904 156451.58494203538, 132926.51789599657 157023.08608502895, 133730.85283800215 158208.4217890352, 136313.19133601338 158144.9216620326, 136207.35779100657 148768.06957499683, 120543.99313095212 147445.15026248991, 103187.99730784446 151745.91685013473)))"
//         };
//         var extract2 = _fixture.Create<RoadNetworkExtractGotRequestedV2>();
//         extract2.Contour = new RoadNetworkExtractGeometry
//         {
//             WKT = "MULTIPOLYGON (((56419.026056587696 165384.45099273324, 60281.950449101627 165331.53422022611, 61609.33318002522 166287.0824880749, 62794.668884024024 165271.08045606688, 64615.005858033895 165355.74729207158, 66435.342832043767 162117.2408150509, 69332.536126427352 163358.13913017884, 71951.91636518389 161611.88563767448, 75087.235135823488 161691.26079642028, 76873.176207698882 159190.94329579547, 78295.682939887047 158277.75158047676, 81631.725611969829 160222.37946972996, 83056.668461859226 160809.12064322457, 85034.82441817224 163340.48970595002, 88521.743392005563 162049.65912429616, 91606.32556117326 162770.51256600395, 92813.335975192487 162083.1871913448, 92897.156142830849 159283.59359215945, 97725.197798915207 158747.14451926202, 100088.92652637511 162049.65912429616, 100994.18433688581 163541.65810829774, 103408.20516492426 163826.64667826146, 103187.99730784446 151745.916850131, 76755.317744970322 152465.85610724613, 46682.339916549623 153054.84300019965, 41126.078804023564 158081.93638770282, 41284.829121522605 162738.61236773431, 47423.1747315526 164008.61490772292, 48904.844361573458 167765.70575524867, 55254.857061602175 168347.7902527377, 56419.026056587696 165384.45099273324)))"
//         };
//         var events = new[] { extract1, extract2 };
//
//         var expected = new List<object>();
//         expected.AddRange(events
//             .Select(requested => new TransactionZoneRecord
//             {
//                 DownloadId = requested.DownloadId,
//                 Description = requested.Description,
//                 Contour = (Geometry)GeometryTranslator.Translate(requested.Contour),
//             }));
//
//         return new TransactionZoneRecordProjection(new NullLoggerFactory())
//             .Scenario()
//             .Given(events.Cast<object>())
//             .Expect(expected);
//     }
// }
