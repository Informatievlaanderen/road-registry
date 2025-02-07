namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes;

using BackOffice.Abstractions.RoadSegments;
using Core;
using FluentAssertions;
using Messages;
using Moq;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Extracts;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;
using ModifyRoadSegmentAttributes = BackOffice.Uploads.ModifyRoadSegmentAttributes;

public class WhenDowngradingCategory : WhenChangeAttributesTestBase
{
    public WhenDowngradingCategory(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task GivenOutlinedRoadSegment_ThenSucceeded()
    {
        // Arrange
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.Category = RoadSegmentCategory.MainRoad;
            });
        var change = request.ChangeRequests.Single();

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.Category = RoadSegmentCategory.EuropeanMainRoad;
        TestData.Segment1Added.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;

        await Given(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(new RoadSegmentId(TestData.Segment1Added.Id)), new RoadNetworkChangesAccepted
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

        translatedChanges.Should().HaveCount(1);
        var roadSegmentId = change.Id;

        var modifyRoadSegmentAttributes = Xunit.Assert.IsType<ModifyRoadSegmentAttributes>(translatedChanges[0]);
        modifyRoadSegmentAttributes.Id.Should().Be(roadSegmentId);
        modifyRoadSegmentAttributes.Category.Should().Be(change.Category);
    }

    [Fact]
    public async Task GivenMeasuredRoadSegment_ThenFailed()
    {
        // Arrange
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.Category = RoadSegmentCategory.MainRoad;
            });

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.Category = RoadSegmentCategory.EuropeanMainRoad;

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
                }
            ],
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        await EditorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, Clock));
        await EditorContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasErrorList("WegcategorieNietVeranderdHuidigeBevatRecentereVersie", "Wegcategorie werd niet gewijzigd voor wegsegment 1 omdat het record reeds een recentere versie bevat.");
    }
}
