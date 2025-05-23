﻿namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments.WhenMergingFakeNodeSegments;

using Framework.Testing;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using LineString = NetTopologySuite.Geometries.LineString;
using RoadSegmentSideAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes;

public partial class GivenIdenticalSegments : RemoveRoadSegmentsTestBase
{
    public GivenIdenticalSegments(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        W5.MaintenanceAuthority.Code = "A";
        W6.MaintenanceAuthority.Code = "A";
    }

    [Fact]
    public async Task ThenNodeIsRemovedAndSegmentsMerged()
    {
        var command = BuildRemoveRoadSegmentsCommand(W1.Id, W2.Id);

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadSegmentRemoved(W5.Id)
            .WithRoadSegmentRemoved(W6.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeRemoved(K2.Id)
            .WithRoadSegmentAdded(new()
            {
                Id = 11,
                TemporaryId = 11,
                Version = 1,
                StartNodeId = K5.Id,
                EndNodeId = K6.Id,
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                AccessRestriction = W5.AccessRestriction,
                Category = W5.Category,
                Status = W5.Status,
                Morphology = W5.Morphology,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = W5.MaintenanceAuthority.Code,
                    Name = W5.MaintenanceAuthority.Name
                },
                Geometry = GeometryTranslator.Translate(new MultiLineString([new LineString([
                    new(0, 0), new(1, 1), new(1, 0)
                ])])),
                GeometryVersion = 1,
                Lanes = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Direction = RoadSegmentLaneDirection.Forward,
                    Count = 1
                }],
                Surfaces = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Type = RoadSegmentSurfaceType.SolidSurface
                }],
                Widths = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Width = new RoadSegmentWidth(3)
                }],
                LeftSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = W5.LeftSide.StreetNameId
                },
                RightSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = W5.RightSide.StreetNameId
                }
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .GivenOrganization(W5.MaintenanceAuthority)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    [Fact]
    public async Task WhenSegmentsHaveSameOppositeNode_ThenTurningLoopNode()
    {
        W3.MaintenanceAuthority.Code = "A";
        W4.MaintenanceAuthority.Code = "A";

        var k20 = CreateRoadNode(20, RoadNodeType.EndNode, 4, 1);
        var k4ToK20 = CreateRoadSegment(20, K4, k20);

        var initialExtraSegment = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithTransactionId(2)
            .WithRoadNodeAdded(k20)
            .WithRoadSegmentAdded(k4ToK20)
            .WithRoadNodeModified(new ()
            {
                Id = K4.Id,
                Type = RoadNodeType.RealNode,
                Version = K4.Version + 1,
                Geometry = K4.Geometry
            })
            .Build();

        var command = BuildRemoveRoadSegmentsCommand(k4ToK20.Id);

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(3)
            .WithRoadSegmentRemoved(k4ToK20.Id)
            .WithRoadNodeRemoved(k20.Id)
            .WithRoadNodeModified(new ()
            {
                Id = K4.Id,
                Type = RoadNodeType.TurningLoopNode,
                Version = K4.Version + 2,
                Geometry = K4.Geometry
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .Given(RoadNetworks.Stream, initialExtraSegment)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    [Fact]
    public async Task WhenOneSegmentWillBeRemoved_ThenNoMerge()
    {
        var command = BuildRemoveRoadSegmentsCommand(W1.Id, W2.Id, W5.Id);

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadSegmentRemoved(W5.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeModified(new()
            {
                Id = K2.Id,
                Type = RoadNodeType.EndNode,
                Version = K2.Version + 1,
                Geometry = K2.Geometry
            })
            .WithRoadNodeModified(new()
            {
                Id = K5.Id,
                Type = RoadNodeType.EndNode,
                Version = K5.Version + 1,
                Geometry = K5.Geometry
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    [Fact]
    public async Task WhenSegmentsHaveDifferentAttribute_Category_ThenNoMerge()
    {
        W5.Category = RoadSegmentCategory.LocalRoad;

        await RunAndExpectNoMerge();
    }
    [Fact]
    public async Task WhenSegmentsHaveDifferentAttribute_Morphology_ThenNoMerge()
    {
        W5.Morphology = RoadSegmentMorphology.FrontageRoad;

        await RunAndExpectNoMerge();
    }
    [Fact]
    public async Task WhenSegmentsHaveDifferentAttribute_GeometryDrawMethod_ThenNoMerge()
    {
        W5.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;

        await RunAndExpectNoMerge();
    }
    [Fact]
    public async Task WhenSegmentsHaveDifferentAttribute_OrganizationId_ThenNoMerge()
    {
        W5.MaintenanceAuthority.Code = W5.Id.ToString();

        await RunAndExpectNoMerge();
    }
    [Fact]
    public async Task WhenSegmentsHaveDifferentAttribute_Status_ThenNoMerge()
    {
        W5.Status = RoadSegmentStatus.OutOfUse;

        await RunAndExpectNoMerge();
    }
    [Fact]
    public async Task WhenSegmentsHaveDifferentAttribute_AccessRestriction_ThenNoMerge()
    {
        W5.AccessRestriction = RoadSegmentAccessRestriction.PrivateRoad;

        await RunAndExpectNoMerge();
    }

    [Fact]
    public async Task WhenSegmentOneIsInOppositeDirectionWithSameStreetNameIds_ThenMerge()
    {
        //merged segment: K5 -> K2 -> K6
        var expectedLeftSide = 1;
        var expectedRightSide = 2;

        // correct direction
        W6.LeftSide = new RoadSegmentSideAttributes
        {
            StreetNameId = expectedLeftSide
        };
        W6.RightSide = new RoadSegmentSideAttributes
        {
            StreetNameId = expectedRightSide
        };

        // opposite direction
        W5.StartNodeId = K2.Id;
        W5.EndNodeId = K5.Id;
        W5.LeftSide = new RoadSegmentSideAttributes
        {
            StreetNameId = expectedRightSide
        };
        W5.RightSide = new RoadSegmentSideAttributes
        {
            StreetNameId = expectedLeftSide
        };

        var command = BuildRemoveRoadSegmentsCommand(W1.Id, W2.Id);

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadSegmentRemoved(W5.Id)
            .WithRoadSegmentRemoved(W6.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeRemoved(K2.Id)
            .WithRoadSegmentAdded(new()
            {
                Id = 11,
                TemporaryId = 11,
                Version = 1,
                StartNodeId = K5.Id,
                EndNodeId = K6.Id,
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                AccessRestriction = W5.AccessRestriction,
                Category = W5.Category,
                Status = W5.Status,
                Morphology = W5.Morphology,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = W5.MaintenanceAuthority.Code,
                    Name = W5.MaintenanceAuthority.Name
                },
                Geometry = GeometryTranslator.Translate(new MultiLineString([new LineString([
                    new(0, 0), new(1, 1), new(1, 0)
                ])])),
                GeometryVersion = 1,
                Lanes = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Direction = RoadSegmentLaneDirection.Forward,
                    Count = 1
                }],
                Surfaces = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Type = RoadSegmentSurfaceType.SolidSurface
                }],
                Widths = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Width = new RoadSegmentWidth(3)
                }],
                LeftSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = expectedLeftSide
                },
                RightSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = expectedRightSide
                }
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .GivenOrganization(W5.MaintenanceAuthority)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    [Fact]
    public async Task WhenSegmentTwoIsInOppositeDirectionWithSameStreetNameIds_ThenMerge()
    {
        var expectedLeftSide = 1;
        var expectedRightSide = 2;

        // correct direction
        W5.LeftSide = new RoadSegmentSideAttributes
        {
            StreetNameId = expectedLeftSide
        };
        W5.RightSide = new RoadSegmentSideAttributes
        {
            StreetNameId = expectedRightSide
        };

        // opposite direction
        W6.StartNodeId = K6.Id;
        W6.EndNodeId = K2.Id;
        W6.LeftSide = new RoadSegmentSideAttributes
        {
            StreetNameId = expectedRightSide
        };
        W6.RightSide = new RoadSegmentSideAttributes
        {
            StreetNameId = expectedLeftSide
        };

        var command = BuildRemoveRoadSegmentsCommand(W1.Id, W2.Id);

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadSegmentRemoved(W5.Id)
            .WithRoadSegmentRemoved(W6.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeRemoved(K2.Id)
            .WithRoadSegmentAdded(new()
            {
                Id = 11,
                TemporaryId = 11,
                Version = 1,
                StartNodeId = K5.Id,
                EndNodeId = K6.Id,
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                AccessRestriction = W5.AccessRestriction,
                Category = W5.Category,
                Status = W5.Status,
                Morphology = W5.Morphology,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = W5.MaintenanceAuthority.Code,
                    Name = W5.MaintenanceAuthority.Name
                },
                Geometry = GeometryTranslator.Translate(new MultiLineString([new LineString([
                    new(0, 0), new(1, 1), new(1, 0)
                ])])),
                GeometryVersion = 1,
                Lanes = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Direction = RoadSegmentLaneDirection.Forward,
                    Count = 1
                }],
                Surfaces = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Type = RoadSegmentSurfaceType.SolidSurface
                }],
                Widths = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Width = new RoadSegmentWidth(3)
                }],
                LeftSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = expectedLeftSide
                },
                RightSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = expectedRightSide
                }
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .GivenOrganization(W5.MaintenanceAuthority)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    private async Task RunAndExpectNoMerge()
    {
        var command = BuildRemoveRoadSegmentsCommand(W1.Id, W2.Id);

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeModified(new()
            {
                Id = K2.Id,
                Type = RoadNodeType.FakeNode,
                Version = K2.Version + 1,
                Geometry = K2.Geometry
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    private Command BuildRemoveRoadSegmentsCommand(params int[] roadSegmentIds)
    {
        return new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(roadSegmentIds)
            .Build();
    }
}
