namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class RoadSegmentModifyTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadSegmentModified()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates()
        };

        // Act
        var problems = segment.Modify(change);

        // Assert
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().HaveCount(1);

        var segmentModified = (RoadSegmentModified)segment.GetChanges().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentModified.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentModified.OriginalId.Should().Be(change.OriginalId ?? change.RoadSegmentId);
        segmentModified.StartNodeId.Should().Be(change.StartNodeId!.Value);
        segmentModified.EndNodeId.Should().Be(change.EndNodeId!.Value);
        segmentModified.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
        segmentModified.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentModified.Category.Should().Be(change.Category);
        segmentModified.Morphology.Should().Be(change.Morphology);
        segmentModified.Status.Should().Be(change.Status);
        segmentModified.StreetNameId.Should().Be(change.StreetNameId);
        segmentModified.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentModified.SurfaceType.Should().Be(change.SurfaceType);
        segmentModified.EuropeanRoadNumbers.Should().BeEquivalentTo(change.EuropeanRoadNumbers);
        segmentModified.NationalRoadNumbers.Should().BeEquivalentTo(change.NationalRoadNumbers);
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = new ModifyRoadSegmentChange
        {
            RoadSegmentId = TestData.Segment1Added.RoadSegmentId
        };

        return Run(scenario => scenario
            .Given(changes => changes)
            .When(changes => changes.Add(change))
            .Throws(new Error("RoadSegmentNotFound", [new("SegmentId", change.RoadSegmentId.ToString())]))
        );
    }

    //TODO-pr test validations RoadSegment.Modify, do Add first

    // [Fact]
    // public Task WhenStartNodeIsMissing_ThenError()
    // {
    //     var change = new ModifyRoadSegmentChange
    //     {
    //         RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
    //         StartNodeId = new RoadNodeId(9)
    //     };
    //
    //     return Run(scenario => scenario
    //         .Given(changes => changes
    //             .Add(TestData.AddStartNode1)
    //             .Add(TestData.AddEndNode1)
    //             .Add(TestData.AddSegment1)
    //         )
    //         .When(changes => changes.Add(change))
    //         .Throws(new Error("RoadSegmentStartNodeMissing", [new("Identifier", "1")]))
    //     );
    // }

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

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>());
        var evt = Fixture.Create<RoadSegmentModified>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Geometry.AsText().Should().Be(evt.Geometry.WKT);
        segment.StartNodeId.Should().Be(evt.StartNodeId);
        segment.EndNodeId.Should().Be(evt.EndNodeId);
        segment.Attributes.GeometryDrawMethod.Should().Be(evt.GeometryDrawMethod);
        segment.Attributes.AccessRestriction.Should().Be(evt.AccessRestriction);
        segment.Attributes.Category.Should().Be(evt.Category);
        segment.Attributes.Morphology.Should().Be(evt.Morphology);
        segment.Attributes.Status.Should().Be(evt.Status);
        segment.Attributes.StreetNameId.Should().Be(evt.StreetNameId);
        segment.Attributes.MaintenanceAuthorityId.Should().Be(evt.MaintenanceAuthorityId);
        segment.Attributes.SurfaceType.Should().Be(evt.SurfaceType);
        segment.Attributes.EuropeanRoadNumbers.Should().BeEquivalentTo(evt.EuropeanRoadNumbers);
        segment.Attributes.NationalRoadNumbers.Should().BeEquivalentTo(evt.NationalRoadNumbers);
    }

}
