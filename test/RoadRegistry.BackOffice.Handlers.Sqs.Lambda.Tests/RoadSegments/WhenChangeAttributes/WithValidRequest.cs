namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes;

using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using Core;
using FluentAssertions;
using Messages;
using NodaTime.Text;
using RoadRegistry.RoadNetwork.ValueObjects;
using RoadRegistry.Tests.BackOffice.Extracts;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;
using AddRoadSegmentToEuropeanRoad = BackOffice.Uploads.AddRoadSegmentToEuropeanRoad;
using AddRoadSegmentToNationalRoad = BackOffice.Uploads.AddRoadSegmentToNationalRoad;
using AddRoadSegmentToNumberedRoad = BackOffice.Uploads.AddRoadSegmentToNumberedRoad;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using RemoveRoadSegmentFromEuropeanRoad = BackOffice.Uploads.RemoveRoadSegmentFromEuropeanRoad;
using RemoveRoadSegmentFromNationalRoad = BackOffice.Uploads.RemoveRoadSegmentFromNationalRoad;
using RemoveRoadSegmentFromNumberedRoad = BackOffice.Uploads.RemoveRoadSegmentFromNumberedRoad;

public class WhenChangeAttributesWithValidRequest : WhenChangeAttributesTestBase
{
    public WhenChangeAttributesWithValidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        // Arrange
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.AccessRestriction = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentAccessRestriction.Parse(TestData.Segment1Added.AccessRestriction));
                change.Category = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentCategory.Parse(TestData.Segment1Added.Category));
                change.MaintenanceAuthority = TestData.ChangedByOrganization;
                change.Morphology = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentMorphology.Parse(TestData.Segment1Added.Morphology));
                change.Status = ObjectProvider.CreateWhichIsDifferentThan(RoadSegmentStatus.Parse(TestData.Segment1Added.Status));
                change.LeftSideStreetNameId = new StreetNameLocalId(WellKnownStreetNameIds.Proposed);
                change.RightSideStreetNameId = StreetNameLocalId.NotApplicable;

                change.EuropeanRoads = ObjectProvider.CreateMany<EuropeanRoadNumber>(1).ToArray();
                change.NationalRoads = ObjectProvider.CreateMany<NationalRoadNumber>(1).ToArray();
                change.NumberedRoads = ObjectProvider.CreateMany<NumberedRoadNumber>(1)
                    .Select(number => new ChangeRoadSegmentNumberedRoadAttribute
                    {
                        Number = number,
                        Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    })
                    .ToArray();
            });
        var change = request.ChangeRequests.Single();

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.LeftSide.StreetNameId = null;
        TestData.Segment1Added.RightSide.StreetNameId = null;

        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            Changes =
            [
                new AcceptedChange
                {
                    RoadSegmentAdded = TestData.Segment1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToEuropeanRoad = new RoadSegmentAddedToEuropeanRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(change.EuropeanRoads!.Single())
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNationalRoad = new RoadSegmentAddedToNationalRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(change.NationalRoads!.Single())
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = 1,
                        TemporaryAttributeId = 1,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = change.NumberedRoads!.Single().Number,
                        Direction = ObjectProvider.CreateWhichIsDifferentThan(change.NumberedRoads!.Single().Direction),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                },
                new AcceptedChange
                {
                    RoadSegmentAddedToNumberedRoad = new RoadSegmentAddedToNumberedRoad
                    {
                        AttributeId = 2,
                        TemporaryAttributeId = 2,
                        SegmentId = TestData.Segment1Added.Id,
                        Number = ObjectProvider.CreateWhichIsDifferentThan(change.NumberedRoads!.Single().Number),
                        Direction = ObjectProvider.Create<RoadSegmentNumberedRoadDirection>(),
                        Ordinal = ObjectProvider.Create<RoadSegmentNumberedRoadOrdinal>()
                    }
                }
            ],
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        await EditorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, Clock));
        await EditorContext.SaveChangesAsync();

        // Act
        var translatedChanges = await HandleRequest(request);

        // Assert
        VerifyThatTicketHasCompleted(new ChangeRoadSegmentAttributesResponse());

        translatedChanges.Should().HaveCount(8);
        var roadSegmentId = change.Id;

        var modifyRoadSegmentAttributes = Xunit.Assert.IsType<ModifyRoadSegment>(translatedChanges[0]);
        modifyRoadSegmentAttributes.Id.Should().Be(roadSegmentId);
        modifyRoadSegmentAttributes.AccessRestriction.Should().Be(change.AccessRestriction);
        modifyRoadSegmentAttributes.Category.Should().Be(change.Category);
        modifyRoadSegmentAttributes.MaintenanceAuthority.Should().Be(change.MaintenanceAuthority);
        modifyRoadSegmentAttributes.Morphology.Should().Be(change.Morphology);
        modifyRoadSegmentAttributes.Status.Should().Be(change.Status);
        modifyRoadSegmentAttributes.LeftSideStreetNameId.Should().Be(change.LeftSideStreetNameId);
        modifyRoadSegmentAttributes.RightSideStreetNameId.Should().Be(change.RightSideStreetNameId);

        var addRoadSegmentToEuropeanRoad = Xunit.Assert.IsType<AddRoadSegmentToEuropeanRoad>(translatedChanges[1]);
        addRoadSegmentToEuropeanRoad.Number.Should().Be(change.EuropeanRoads!.Single());

        var removeRoadSegmentFromEuropeanRoad = Xunit.Assert.IsType<RemoveRoadSegmentFromEuropeanRoad>(translatedChanges[2]);
        removeRoadSegmentFromEuropeanRoad.SegmentId.Should().Be(roadSegmentId);

        var addRoadSegmentToNationalRoad = Xunit.Assert.IsType<AddRoadSegmentToNationalRoad>(translatedChanges[3]);
        addRoadSegmentToNationalRoad.Number.Should().Be(change.NationalRoads!.Single());

        var removeRoadSegmentFromNationalRoad = Xunit.Assert.IsType<RemoveRoadSegmentFromNationalRoad>(translatedChanges[4]);
        removeRoadSegmentFromNationalRoad.SegmentId.Should().Be(roadSegmentId);

        var removeRoadSegmentFromNumberedRoadReplacement = Xunit.Assert.IsType<RemoveRoadSegmentFromNumberedRoad>(translatedChanges[5]);
        removeRoadSegmentFromNumberedRoadReplacement.SegmentId.Should().Be(roadSegmentId);
        removeRoadSegmentFromNumberedRoadReplacement.Number.Should().Be(change.NumberedRoads!.Single().Number);

        var addRoadSegmentToNumberedRoadReplacement = Xunit.Assert.IsType<AddRoadSegmentToNumberedRoad>(translatedChanges[6]);
        addRoadSegmentToNumberedRoadReplacement.Number.Should().Be(change.NumberedRoads!.Single().Number);

        var removeRoadSegmentFromNumberedRoad = Xunit.Assert.IsType<RemoveRoadSegmentFromNumberedRoad>(translatedChanges[7]);
        removeRoadSegmentFromNumberedRoad.SegmentId.Should().Be(roadSegmentId);
    }
}
