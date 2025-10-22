namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline;

using BackOffice.Abstractions.RoadSegmentsOutline;
using Core;
using FluentAssertions;
using Messages;
using NodaTime.Text;
using RoadSegment.ValueObjects;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public class WithValidRequest : WhenDeleteOutlineTestBase
{
    public WithValidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        // Arrange
        var request = new DeleteRoadSegmentOutlineRequest(
            new RoadSegmentId(TestData.Segment1Added.Id)
        );

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        TestData.Segment1Added.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined.ToString();
        TestData.Segment1Added.StartNodeId = 0;
        TestData.Segment1Added.EndNodeId = 0;

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

        // Act
        await HandleRequest(request);

        // Assert
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        await VerifyThatTicketHasCompleted(roadSegmentId);

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        command.Changes.Single().RoadSegmentRemoved.Id.Should().Be(roadSegmentId);
    }
}
