namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline;

using AutoFixture;
using BackOffice.Abstractions.RoadSegmentsOutline;
using Core;
using Messages;
using NodaTime.Text;
using RoadRegistry.Extracts.Schema;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public class GivenInwinningRoadSegment : WhenDeleteOutlineTestBase
{
    public GivenInwinningRoadSegment(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ThenError()
    {
        // Arrange
        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        var request = new DeleteRoadSegmentOutlineRequest(
            roadSegmentId
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

        await Given(RoadNetworkStreamNameProvider.ForOutlinedRoadSegment(roadSegmentId), new RoadNetworkChangesAccepted
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

        var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = roadSegmentId,
            Completed = ObjectProvider.Create<bool>()
        });
        await extractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request, extractsDbContext: extractsDbContext);

        // Assert
        VerifyThatTicketHasError("RoadSegmentIsInInwinning", $"Het wegsegment met id {roadSegmentId} heeft de inwinningsstatus 'locked' of 'compleet'.");
    }
}
