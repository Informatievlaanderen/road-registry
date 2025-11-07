namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.RoadNetwork.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegmentModified = RoadRegistry.RoadSegment.Events.RoadSegmentModified;

//TODO-pr aggregates (roadsegment/roadnode/gradeseparatedjunction) test each action: validation+event
//TODO-pr unit test domein: RoadNetworkChangeTests en RoadSegmentModifyTests

//TODO-pr voeg rest vd domein changes uit ChangeRoadNetworkCommand toe

public class RoadSegmentModifyTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenRoadSegmentModified()
    {
        var change = new ModifyRoadSegmentChange
        {
            Id = new RoadSegmentId(TestData.Segment1Added.Id),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()]
        };

        return Run(scenario => scenario
                .Given(changes => changes
                    .Add(TestData.AddStartNode1)
                    .Add(TestData.AddEndNode1)
                    .Add(TestData.AddSegment1)
                )
                .When(changes => changes.Add(change))
                .Then(new RoadSegmentModified
                {
                    Id = change.Id,
                    Geometry = TestData.Segment1Added.Geometry,
                    StartNodeId = TestData.Segment1Added.StartNodeId,
                    EndNodeId = TestData.Segment1Added.EndNodeId,
                    GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
                    AccessRestriction = TestData.Segment1Added.AccessRestriction,
                    Category = TestData.Segment1Added.Category,
                    Morphology = TestData.Segment1Added.Morphology,
                    Status = TestData.Segment1Added.Status,
                    StreetNameId = TestData.Segment1Added.StreetNameId,
                    MaintenanceAuthorityId = TestData.Segment1Added.MaintenanceAuthorityId,
                    SurfaceType = TestData.Segment1Added.SurfaceType,
                    EuropeanRoadNumbers = change.EuropeanRoadNumbers,
                    NationalRoadNumbers = []
                })
        );
    }

    [Fact]
    public Task WhenUnknownRoadNodeId_ThenError()
    {
        var change = new ModifyRoadSegmentChange
        {
            Id = new RoadSegmentId(TestData.Segment1Added.Id),
            StartNodeId = new RoadNodeId(9)
        };

        return Run(scenario => scenario
                .Given(changes => changes
                    .Add(TestData.AddStartNode1)
                    .Add(TestData.AddEndNode1)
                    .Add(TestData.AddSegment1)
                )
                .When(changes => changes.Add(change))
                .Throws(
                    new Error("RoadSegmentStartNodeMissing", [new("Identifier", "1")])
                )
        );
    }

    // [Fact]
    // public Task when_modifying_a_segment_geometry_with_length_at_least_100000()
    // {
    //     var startPoint = new Point(new CoordinateM(0.0, 0.0, 0.0))
    //     {
    //         SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
    //     };
    //     var endPoint = new Point(new CoordinateM(100000.0, 0.0, 100000.0))
    //     {
    //         SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
    //     };
    //     var lineString = new LineString(
    //         new CoordinateArraySequence([startPoint.Coordinate, endPoint.Coordinate]),
    //         GeometryConfiguration.GeometryFactory
    //     );
    //     var geometry = new MultiLineString([lineString])
    //     {
    //         SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
    //     };
    //
    //     var geometryLength = (decimal)geometry.Length;
    //
    //     var modifyRoadSegment = new ModifyRoadSegment
    //     {
    //         Id = TestData.Segment1Added.Id,
    //         GeometryDrawMethod = TestData.Segment1Added.GeometryDrawMethod,
    //         Geometry = GeometryTranslator.Translate(geometry),
    //         Lanes = new[]
    //         {
    //             new RequestedRoadSegmentLaneAttribute
    //             {
    //                 AttributeId = 1,
    //                 FromPosition = 0,
    //                 ToPosition = geometryLength,
    //                 Count = ObjectProvider.Create<RoadSegmentLaneCount>(),
    //                 Direction = ObjectProvider.Create<RoadSegmentLaneDirection>()
    //             }
    //         },
    //         Surfaces = new[]
    //         {
    //             new RequestedRoadSegmentSurfaceAttribute
    //             {
    //                 AttributeId = 1,
    //                 FromPosition = 0,
    //                 ToPosition = geometryLength,
    //                 Type = ObjectProvider.Create<RoadSegmentSurfaceType>()
    //             }
    //         },
    //         Widths = new[]
    //         {
    //             new RequestedRoadSegmentWidthAttribute
    //             {
    //                 AttributeId = 1,
    //                 FromPosition = 0,
    //                 ToPosition = geometryLength,
    //                 Width = ObjectProvider.Create<RoadSegmentWidth>()
    //             }
    //         }
    //     };
    //
    //     return Run(scenario => scenario
    //         .Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
    //             new ImportedOrganization
    //             {
    //                 Code = TestData.ChangedByOrganization,
    //                 Name = TestData.ChangedByOrganizationName,
    //                 When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
    //             }
    //         ).Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
    //         {
    //             RequestId = TestData.RequestId,
    //             Reason = TestData.ReasonForChange,
    //             Operator = TestData.ChangedByOperator,
    //             OrganizationId = TestData.ChangedByOrganization,
    //             Organization = TestData.ChangedByOrganizationName,
    //             Changes = new[]
    //             {
    //                 new AcceptedChange
    //                 {
    //                     RoadNodeAdded = TestData.StartNode1Added
    //                 },
    //                 new AcceptedChange
    //                 {
    //                     RoadNodeAdded = TestData.EndNode1Added
    //                 },
    //                 new AcceptedChange
    //                 {
    //                     RoadSegmentAdded = TestData.Segment1Added
    //                 }
    //             },
    //             When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
    //         })
    //         .When(TheOperator.ChangesTheRoadNetwork(
    //             TestData.RequestId, TestData.ReasonForChange, TestData.ChangedByOperator, TestData.ChangedByOrganization,
    //             new RequestedChange
    //             {
    //                 ModifyRoadSegment = modifyRoadSegment
    //             }
    //         ))
    //         .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
    //         {
    //             RequestId = TestData.RequestId,
    //             Reason = TestData.ReasonForChange,
    //             Operator = TestData.ChangedByOperator,
    //             OrganizationId = TestData.ChangedByOrganization,
    //             Organization = TestData.ChangedByOrganizationName,
    //             TransactionId = new TransactionId(1),
    //             Changes =
    //             [
    //                 new RejectedChange
    //                 {
    //                     ModifyRoadSegment = new ModifyRoadSegment
    //                     {
    //                         Id = TestData.Segment1Added.Id,
    //                         Geometry = GeometryTranslator.Translate(geometry),
    //                         GeometryDrawMethod = modifyRoadSegment.GeometryDrawMethod,
    //                         Lanes =
    //                         [
    //                             new RequestedRoadSegmentLaneAttribute
    //                             {
    //                                 AttributeId = 1,
    //                                 FromPosition = 0,
    //                                 ToPosition = geometryLength,
    //                                 Count = modifyRoadSegment.Lanes.Single().Count,
    //                                 Direction = modifyRoadSegment.Lanes.Single().Direction
    //                             }
    //                         ],
    //                         Surfaces =
    //                         [
    //                             new RequestedRoadSegmentSurfaceAttribute
    //                             {
    //                                 AttributeId = 1,
    //                                 FromPosition = 0,
    //                                 ToPosition = geometryLength,
    //                                 Type = modifyRoadSegment.Surfaces.Single().Type,
    //                             }
    //                         ],
    //                         Widths =
    //                         [
    //                             new RequestedRoadSegmentWidthAttribute
    //                             {
    //                                 AttributeId = 1,
    //                                 FromPosition = 0,
    //                                 ToPosition = geometryLength,
    //                                 Width = modifyRoadSegment.Widths.Single().Width,
    //                             }
    //                         ]
    //                     },
    //                     Problems =
    //                     [
    //                         new Problem
    //                         {
    //                             Reason = "RoadSegmentGeometryLengthTooLong",
    //                             Parameters =
    //                             [
    //                                 new("Identifier", TestData.Segment1Added.Id.ToString()),
    //                                 new("TooLongSegmentLength", "100000")
    //                             ]
    //                         }
    //                     ]
    //                 }
    //             ],
    //             When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
    //         }));
}
