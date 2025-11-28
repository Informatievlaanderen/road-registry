namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions.RoadSegments;
using Core;
using Messages;
using NodaTime.Text;
using RoadRegistry.RoadNetwork.ValueObjects;
using RoadRegistry.Tests.BackOffice.Extracts;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public class WhenLinkingStreetName : WhenChangeAttributesTestBase
{
    public WhenLinkingStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task WhenLeftStreetNameHasNotActiveStatus_ThenFailure()
    {
        // Arrange
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.LeftSideStreetNameId = new StreetNameLocalId(WellKnownStreetNameIds.Retired);
            });

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

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
        VerifyThatTicketHasError(
            "LinkerstraatnaamNietVoorgesteldOfInGebruik",
            $"De linkerstraatnaam voor wegsegment met id {request.ChangeRequests.Single().Id} moet status 'voorgesteld' of 'in gebruik' hebben.",
            new Dictionary<string, object>
            {
                { "WegsegmentId", TestData.Segment1Added.Id }
            });
    }

    [Fact]
    public async Task WhenLeftStreetNameNotFound_ThenFailure()
    {
        // Arrange
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.LeftSideStreetNameId = new StreetNameLocalId(int.MaxValue);
            });

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

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
        VerifyThatTicketHasError(
            "LinkerstraatnaamNietGekend",
            $"De linkerstraatnaam voor het wegsegment met id {request.ChangeRequests.Single().Id} is niet gekend in het Straatnamenregister.",
            new Dictionary<string, object>
            {
                { "WegsegmentId", TestData.Segment1Added.Id }
            });
    }

    [Fact]
    public async Task WhenRightStreetNameHasNotActiveStatus_ThenFailure()
    {
        // Arrange
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.RightSideStreetNameId = new StreetNameLocalId(WellKnownStreetNameIds.Retired);
            });

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

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
        VerifyThatTicketHasError(
            "RechterstraatnaamNietVoorgesteldOfInGebruik",
            $"De rechterstraatnaam voor wegsegment met id {request.ChangeRequests.Single().Id} moet status 'voorgesteld' of 'in gebruik' hebben.",
            new Dictionary<string, object>
            {
                { "WegsegmentId", TestData.Segment1Added.Id }
            });
    }

    [Fact]
    public async Task WhenRightStreetNameNotFound_ThenFailure()
    {
        // Arrange
        var request = new ChangeRoadSegmentAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                change.RightSideStreetNameId = new StreetNameLocalId(int.MaxValue);
            });

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

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
        VerifyThatTicketHasError(
            "RechterstraatnaamNietGekend",
            $"De rechterstraatnaam voor het wegsegment met id {request.ChangeRequests.Single().Id} is niet gekend in het Straatnamenregister.",
            new Dictionary<string, object>
            {
                { "WegsegmentId", TestData.Segment1Added.Id }
            });
    }
}
