namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using AutoFixture;
using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadSegment.ValueObjects;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSideAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public class GivenOutlinedSegment : RoadNetworkTestBase
{
    private readonly Fixture _fixture;

    private readonly RoadSegmentAdded _outlinedSegment;
    private readonly RoadNetworkChangesAccepted _initialRoadNetwork;

    public GivenOutlinedSegment(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _fixture = TestData.ObjectProvider;

        _outlinedSegment = CreateRoadSegment();

        _initialRoadNetwork = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadSegmentAdded(_outlinedSegment)
            .Build();
    }

    private RoadSegmentAdded CreateRoadSegment()
    {
        var roadSegmentGeometry = _fixture.Create<RoadSegmentGeometry>();
        var lineString = GeometryTranslator.Translate(roadSegmentGeometry);

        return new RoadSegmentAdded
        {
            Id = 1,
            Version = 1,
            TemporaryId = 1,
            Geometry = roadSegmentGeometry,
            GeometryVersion = 1,
            MaintenanceAuthority = new MaintenanceAuthority
            {
                Code = TestData.ChangedByOrganization,
                Name = TestData.ChangedByOrganizationName
            },
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Morphology = RoadSegmentMorphology.Unknown,
            Status = RoadSegmentStatus.Unknown,
            Category = RoadSegmentCategory.Unknown,
            AccessRestriction = RoadSegmentAccessRestriction.PublicRoad,
            LeftSide = new RoadSegmentSideAttributes
            {
                StreetNameId = StreetNameLocalId.NotApplicable
            },
            RightSide = new RoadSegmentSideAttributes
            {
                StreetNameId = StreetNameLocalId.NotApplicable
            },
            Lanes =
            [
                new RoadSegmentLaneAttributes
                {
                    AttributeId = 1,
                    Direction = RoadSegmentLaneDirection.Unknown,
                    Count = 1,
                    FromPosition = 0,
                    ToPosition = (decimal)lineString.Length,
                    AsOfGeometryVersion = 1
                }
            ],
            Widths =
            [
                new RoadSegmentWidthAttributes
                {
                    AttributeId = 1,
                    Width = _fixture.Create<RoadSegmentWidth>(),
                    FromPosition = 0,
                    ToPosition = (decimal)lineString.Length,
                    AsOfGeometryVersion = 1
                }
            ],
            Surfaces =
            [
                new RoadSegmentSurfaceAttributes
                {
                    AttributeId = 1,
                    Type = _fixture.Create<RoadSegmentSurfaceType>(),
                    FromPosition = 0,
                    ToPosition = (decimal)lineString.Length,
                    AsOfGeometryVersion = 1
                }
            ]
        };
    }

    [Fact]
    public async Task WhenRemovingSegment_ThenSegmentIsRemoved()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Ids = [_outlinedSegment.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids, RoadSegmentGeometryDrawMethod.Outlined)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(_outlinedSegment.Id, RoadSegmentGeometryDrawMethod.Outlined)
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(_outlinedSegment.Id)), _initialRoadNetwork)
                .When(command)
                .Then(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(_outlinedSegment.Id)), expected)
        );
    }
}
