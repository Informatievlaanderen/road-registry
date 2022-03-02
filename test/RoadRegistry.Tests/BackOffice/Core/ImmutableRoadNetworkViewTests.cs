namespace RoadRegistry.BackOffice.Core
{
    using System;
    using AutoFixture;
    using FluentAssertions;
    using Xunit;

    public class ImmutableRoadNetworkViewTests
    {
        public Fixture Fixture { get; }
        public RoadNetworkTestHelpers RoadNetworkFixtures { get; }


        public ImmutableRoadNetworkViewTests()
        {
            Fixture = new Fixture();

            Fixture.Customize<Messages.RoadSegmentAddedToNationalRoad>(
                composer =>
                    composer.FromFactory(random =>
                        new Messages.RoadSegmentAddedToNationalRoad
                        {
                            AttributeId = Fixture.Create<int>(),
                            TemporaryAttributeId = Fixture.Create<int>(),
                            Number = Fixture.Create<string>(),
                            SegmentId = Fixture.Create<int>(),
                        }).OmitAutoProperties()
            );

            Fixture.Customize<Messages.RoadNetworkChangesAccepted>(
                composer =>
                    composer.FromFactory(random =>
                        new Messages.RoadNetworkChangesAccepted
                        {
                            Operator = Fixture.Create<string>(),
                            Organization = Fixture.Create<string>(),
                            Reason = Fixture.Create<string>(),
                            When = Fixture.Create<string>(),
                            OrganizationId = Fixture.Create<string>(),
                            RequestId = Fixture.Create<string>(),
                            TransactionId = Fixture.Create<int>(),
                            Changes = Array.Empty<Messages.AcceptedChange>()
                        }));

            RoadNetworkFixtures = RoadNetworkTestHelpers.Create();
        }

        [Fact]
        public void RoadSegmentAddedToNationalRoad_AddsRoadSegmentToNationalRoad()
        {
            // GIVEN
            IRoadNetworkView roadNetwork = ImmutableRoadNetworkView.Empty;

            var given = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            given.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAdded = RoadNetworkFixtures.Segment1Added,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            roadNetwork = roadNetwork.RestoreFromEvent(given);

            // WHEN
            var roadSegmentAddedToNationalRoad = new Messages.RoadSegmentAddedToNationalRoad
            {
                Number = "A001",
                AttributeId = 1,
                SegmentId = RoadNetworkFixtures.Segment1Added.Id,
                TemporaryAttributeId = 1
            };
            var acceptedChange = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            acceptedChange.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAddedToNationalRoad = roadSegmentAddedToNationalRoad,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            var result = roadNetwork.RestoreFromEvent(acceptedChange);

            // THEN
            var actualSegment = result.Segments[new RoadSegmentId(RoadNetworkFixtures.Segment1Added.Id)];
            actualSegment.PartOfNationalRoads.Contains(NationalRoadNumber.Parse(roadSegmentAddedToNationalRoad.Number)).Should().BeTrue();
        }

        [Fact]
        public void RoadSegmentRemovedFromNationalRoad_RemovesRoadSegmentFromNationalRoad()
        {
            // GIVEN
            IRoadNetworkView roadNetwork = ImmutableRoadNetworkView.Empty;

            var given = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            var roadSegmentAddedToNationalRoad = new Messages.RoadSegmentAddedToNationalRoad
            {
                Number = "A001",
                AttributeId = 1,
                SegmentId = RoadNetworkFixtures.Segment1Added.Id,
                TemporaryAttributeId = 1
            };

            given.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAdded = RoadNetworkFixtures.Segment1Added,
                    Problems = Array.Empty<Messages.Problem>()
                },
                new Messages.AcceptedChange
                {
                    RoadSegmentAddedToNationalRoad = roadSegmentAddedToNationalRoad,
                    Problems = Array.Empty<Messages.Problem>()
                },
            };
            roadNetwork = roadNetwork.RestoreFromEvent(given);

            // WHEN
            var acceptedChange = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            var roadSegmentRemovedFromNationalRoad = new Messages.RoadSegmentRemovedFromNationalRoad
            {
                Number = roadSegmentAddedToNationalRoad.Number,
                AttributeId = roadSegmentAddedToNationalRoad.AttributeId,
                SegmentId = roadSegmentAddedToNationalRoad.SegmentId,
            };
            acceptedChange.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentRemovedFromNationalRoad = roadSegmentRemovedFromNationalRoad,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            var result = roadNetwork.RestoreFromEvent(acceptedChange);

            // THEN
            var actualSegment = result.Segments[new RoadSegmentId(RoadNetworkFixtures.Segment1Added.Id)];
            actualSegment.PartOfNationalRoads.Contains(NationalRoadNumber.Parse(roadSegmentAddedToNationalRoad.Number)).Should().BeFalse();
        }

        [Fact]
        public void RoadSegmentAddedToNumberedRoad_AddsRoadSegmentToNumberedRoad()
        {
            // GIVEN
            IRoadNetworkView roadNetwork = ImmutableRoadNetworkView.Empty;

            var given = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            given.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAdded = RoadNetworkFixtures.Segment1Added,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            roadNetwork = roadNetwork.RestoreFromEvent(given);

            // WHEN
            var roadSegmentAddedToNumberedRoad = new Messages.RoadSegmentAddedToNumberedRoad
            {
                Number = "A0000001",
                AttributeId = 1,
                SegmentId = RoadNetworkFixtures.Segment1Added.Id,
                TemporaryAttributeId = 1
            };
            var acceptedChange = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            acceptedChange.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = roadSegmentAddedToNumberedRoad,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            var result = roadNetwork.RestoreFromEvent(acceptedChange);

            // THEN
            var actualSegment = result.Segments[new RoadSegmentId(RoadNetworkFixtures.Segment1Added.Id)];
            actualSegment.PartOfNumberedRoads.Contains(NumberedRoadNumber.Parse(roadSegmentAddedToNumberedRoad.Number)).Should().BeTrue();
        }

        [Fact]
        public void RoadSegmentRemovedFromNumberedRoad_RemovesRoadSegmentFromNumberedRoad()
        {
            // GIVEN
            IRoadNetworkView roadNetwork = ImmutableRoadNetworkView.Empty;

            var given = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            var roadSegmentAddedToNumberedRoad = new Messages.RoadSegmentAddedToNumberedRoad
            {
                Number = "A0000001",
                AttributeId = 1,
                SegmentId = RoadNetworkFixtures.Segment1Added.Id,
                TemporaryAttributeId = 1
            };

            given.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAdded = RoadNetworkFixtures.Segment1Added,
                    Problems = Array.Empty<Messages.Problem>()
                },
                new Messages.AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = roadSegmentAddedToNumberedRoad,
                    Problems = Array.Empty<Messages.Problem>()
                },
            };
            roadNetwork = roadNetwork.RestoreFromEvent(given);

            // WHEN
            var acceptedChange = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            var roadSegmentRemovedFromNumberedRoad = new Messages.RoadSegmentRemovedFromNumberedRoad
            {
                Number = roadSegmentAddedToNumberedRoad.Number,
                AttributeId = roadSegmentAddedToNumberedRoad.AttributeId,
                SegmentId = roadSegmentAddedToNumberedRoad.SegmentId,
            };
            acceptedChange.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentRemovedFromNumberedRoad = roadSegmentRemovedFromNumberedRoad,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            var result = roadNetwork.RestoreFromEvent(acceptedChange);

            // THEN
            var actualSegment = result.Segments[new RoadSegmentId(RoadNetworkFixtures.Segment1Added.Id)];
            actualSegment.PartOfNumberedRoads.Contains(NumberedRoadNumber.Parse(roadSegmentAddedToNumberedRoad.Number)).Should().BeFalse();
        }

        [Fact]
        public void RoadSegmentAddedToEuropeanRoad_AddsRoadSegmentToEuropeanRoad()
        {
            // GIVEN
            IRoadNetworkView roadNetwork = ImmutableRoadNetworkView.Empty;

            var given = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            given.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAdded = RoadNetworkFixtures.Segment1Added,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            roadNetwork = roadNetwork.RestoreFromEvent(given);

            // WHEN
            var roadSegmentAddedToEuropeanRoad = new Messages.RoadSegmentAddedToEuropeanRoad
            {
                Number = EuropeanRoadNumber.E17,
                AttributeId = 1,
                SegmentId = RoadNetworkFixtures.Segment1Added.Id,
                TemporaryAttributeId = 1
            };
            var acceptedChange = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            acceptedChange.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAddedToEuropeanRoad = roadSegmentAddedToEuropeanRoad,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            var result = roadNetwork.RestoreFromEvent(acceptedChange);

            // THEN
            var actualSegment = result.Segments[new RoadSegmentId(RoadNetworkFixtures.Segment1Added.Id)];
            actualSegment.PartOfEuropeanRoads.Contains(EuropeanRoadNumber.Parse(roadSegmentAddedToEuropeanRoad.Number)).Should().BeTrue();
        }

        [Fact]
        public void RoadSegmentRemovedFromEuropeanRoad_RemovesRoadSegmentFromEuropeanRoad()
        {
            // GIVEN
            IRoadNetworkView roadNetwork = ImmutableRoadNetworkView.Empty;

            var given = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            var roadSegmentAddedToEuropeanRoad = new Messages.RoadSegmentAddedToEuropeanRoad
            {
                Number = EuropeanRoadNumber.E17,
                AttributeId = 1,
                SegmentId = RoadNetworkFixtures.Segment1Added.Id,
                TemporaryAttributeId = 1
            };

            given.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentAdded = RoadNetworkFixtures.Segment1Added,
                    Problems = Array.Empty<Messages.Problem>()
                },
                new Messages.AcceptedChange
                {
                    RoadSegmentAddedToEuropeanRoad = roadSegmentAddedToEuropeanRoad,
                    Problems = Array.Empty<Messages.Problem>()
                },
            };
            roadNetwork = roadNetwork.RestoreFromEvent(given);

            // WHEN
            var acceptedChange = Fixture.Create<Messages.RoadNetworkChangesAccepted>();
            var roadSegmentRemovedFromEuropeanRoad = new Messages.RoadSegmentRemovedFromEuropeanRoad
            {
                Number = roadSegmentAddedToEuropeanRoad.Number,
                AttributeId = roadSegmentAddedToEuropeanRoad.AttributeId,
                SegmentId = roadSegmentAddedToEuropeanRoad.SegmentId,
            };
            acceptedChange.Changes = new[]
            {
                new Messages.AcceptedChange
                {
                    RoadSegmentRemovedFromEuropeanRoad = roadSegmentRemovedFromEuropeanRoad,
                    Problems = Array.Empty<Messages.Problem>()
                }
            };
            var result = roadNetwork.RestoreFromEvent(acceptedChange);

            // THEN
            var actualSegment = result.Segments[new RoadSegmentId(RoadNetworkFixtures.Segment1Added.Id)];
            actualSegment.PartOfEuropeanRoads.Contains(EuropeanRoadNumber.Parse(roadSegmentAddedToEuropeanRoad.Number)).Should().BeFalse();
        }
    }
}
