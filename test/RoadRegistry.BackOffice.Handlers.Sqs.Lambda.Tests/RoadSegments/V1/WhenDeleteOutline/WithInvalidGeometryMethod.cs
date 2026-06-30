namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline;

using Abstractions.RoadSegmentsOutline;
using Core;
using Messages;
using NodaTime.Text;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public class WithInvalidGeometryMethod : WhenDeleteOutlineTestBase
{
    public WithInvalidGeometryMethod(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task GivenMeasuredRoadSegment_ThenFailed()
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

        TestData.Segment1Added.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured.ToString();

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
        VerifyThatTicketHasError("NotFound", "Dit wegsegment bestaat niet of heeft niet de geometriemethode 'ingeschetst'.");
    }
}
