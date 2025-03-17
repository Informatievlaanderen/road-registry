namespace RoadRegistry.BackOffice.CommandHost.Tests
{
    using System.Linq;
    using AutoFixture;
    using FluentAssertions;
    using Messages;
    using RoadRegistry.BackOffice.Core;
    using RoadRegistry.Tests.BackOffice;
    using RoadRegistry.Tests.BackOffice.Scenarios;
    using Xunit.Sdk;
    using AddGradeSeparatedJunction = Messages.AddGradeSeparatedJunction;
    using AddRoadNode = Messages.AddRoadNode;
    using ModifyGradeSeparatedJunction = Messages.ModifyGradeSeparatedJunction;
    using ModifyRoadNode = Messages.ModifyRoadNode;
    using RemoveGradeSeparatedJunction = Messages.RemoveGradeSeparatedJunction;
    using RemoveOutlinedRoadSegmentFromRoadNetwork = Messages.RemoveOutlinedRoadSegmentFromRoadNetwork;
    using RemoveRoadNode = Messages.RemoveRoadNode;
    using RemoveRoadSegments = Messages.RemoveRoadSegments;

    public class RequestedChangesConverterTests
    {
        private static readonly Type[] ChangeTypesThatAlwaysBelongInDefaultRoadNetwork =
        [
            typeof(AddGradeSeparatedJunction),
            typeof(AddRoadNode),
            typeof(ModifyGradeSeparatedJunction),
            typeof(ModifyRoadNode),
            typeof(RemoveGradeSeparatedJunction),
            typeof(RemoveRoadNode),
            typeof(RemoveOutlinedRoadSegmentFromRoadNetwork)
        ];

        [Fact]
        public void EnsureRequestedChangesAreCorrectlySplitByStream()
        {
            CheckIfRequestedChangesAreCorrectlySplitByStream(ChangeTypesThatAlwaysBelongInDefaultRoadNetwork);
        }

        [Fact]
        public void WhenChangeTypeIsNotHandledItShouldFail()
        {
            Assert.Throws<FailException>(() =>
            {
                CheckIfRequestedChangesAreCorrectlySplitByStream(ChangeTypesThatAlwaysBelongInDefaultRoadNetwork.Except([typeof(AddRoadNode)]).ToArray());
            });
        }

        [Fact]
        public void WhenRemoveRoadSegmentsWithMultipleOutlinedIds_ThenSplitPerId()
        {
            var requestedChanges = new []
            {
                new RequestedChange
                {
                    RemoveRoadSegments = new RemoveRoadSegments
                    {
                        GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                        Ids = [1,2]
                    }
                },
                new RequestedChange
                {
                    RemoveRoadSegments = new RemoveRoadSegments
                    {
                        GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
                        Ids = [3,4]
                    }
                }
            };

            var streamChanges = RequestedChangesConverter.SplitChangesByRoadNetworkStream(requestedChanges);
            streamChanges.Keys.Should().HaveCount(3);

            var change1 = (RemoveRoadSegments)streamChanges.Values.ToList()[0].Flatten().Single();
            var change2 = (RemoveRoadSegments)streamChanges.Values.ToList()[1].Flatten().Single();
            var change3 = (RemoveRoadSegments)streamChanges.Values.ToList()[2].Flatten().Single();

            change1.Ids.Should().BeEquivalentTo([1, 2]);
            change2.Ids.Should().BeEquivalentTo([3]);
            change3.Ids.Should().BeEquivalentTo([4]);
        }

        private static void CheckIfRequestedChangesAreCorrectlySplitByStream(Type[] changeTypesThatAlwaysBelongInDefaultRoadNetwork)
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            fixture.CustomizeRoadSegmentOutlineGeometryDrawMethod();

            fixture.Freeze<RoadSegmentId>();
            var roadSegmentId = fixture.Create<RoadSegmentId>();

            fixture.CustomizeAddGradeSeparatedJunction();
            fixture.CustomizeAddRoadNode();
            fixture.CustomizeAddRoadSegment();
            fixture.CustomizeAddRoadSegmentToEuropeanRoad();
            fixture.CustomizeAddRoadSegmentToNationalRoad();
            fixture.CustomizeAddRoadSegmentToNumberedRoad();
            fixture.CustomizeModifyGradeSeparatedJunction();
            fixture.CustomizeModifyRoadNode();
            fixture.CustomizeModifyRoadSegment();
            fixture.CustomizeModifyRoadSegmentAttributes();
            fixture.CustomizeModifyRoadSegmentGeometry();
            fixture.CustomizeRemoveGradeSeparatedJunction();
            fixture.CustomizeRemoveRoadNode();
            fixture.CustomizeRemoveRoadSegment();
            fixture.CustomizeRemoveRoadSegments();
            fixture.CustomizeRemoveOutlinedRoadSegment();
            fixture.CustomizeRemoveOutlinedRoadSegmentFromRoadNetwork();
            fixture.CustomizeRemoveRoadSegmentFromEuropeanRoad();
            fixture.CustomizeRemoveRoadSegmentFromNationalRoad();
            fixture.CustomizeRemoveRoadSegmentFromNumberedRoad();

            var unhandledChangeTypes = new List<Type>();

            foreach (var requestedChange in fixture.CreateAllRequestedChanges())
            {
                var streamChanges = RequestedChangesConverter.SplitChangesByRoadNetworkStream([requestedChange]);
                Assert.Single(streamChanges.Keys);
                Assert.Single(streamChanges.Values);
                Assert.Single(streamChanges.Values.Single());

                var changePropertyType = requestedChange.Flatten().GetType();

                var actualStreamName = streamChanges.Keys.Single();
                var expectedStreamName = changeTypesThatAlwaysBelongInDefaultRoadNetwork.Contains(changePropertyType)
                    ? RoadNetworkStreamNameProvider.Default
                    : RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(roadSegmentId);

                if (expectedStreamName != actualStreamName)
                {
                    unhandledChangeTypes.Add(changePropertyType);
                }
            }

            if (unhandledChangeTypes.Any())
            {
                Assert.Fail($"The following change properties do not end up in the expected stream. Either it is not implemented in '{nameof(RequestedChangesConverter)}.{nameof(RequestedChangesConverter.SplitChangesByRoadNetworkStream)}', or it should be added to the exclusion list '{nameof(changeTypesThatAlwaysBelongInDefaultRoadNetwork)}' when it has no property which holds the road segment geometry draw method, or there is a missing `CustomizeXXX` extension method to provide a valid road segment geometry draw method value.\n- " + string.Join("\n- ", unhandledChangeTypes.Select(x => x.FullName)));
            }
        }
    }
}
