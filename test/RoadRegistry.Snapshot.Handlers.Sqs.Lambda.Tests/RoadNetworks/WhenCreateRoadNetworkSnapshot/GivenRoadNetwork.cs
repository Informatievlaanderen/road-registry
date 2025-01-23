namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenCreateRoadNetworkSnapshot;

using AutoFixture;
using BackOffice.Abstractions.RoadNetworks;
using BackOffice.Core;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Handlers;
using Moq;
using NodaTime.Text;
using Requests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.Tests.Framework;
using Sqs.RoadNetworks;
using Xunit.Abstractions;
using AcceptedChange = BackOffice.Messages.AcceptedChange;

public class GivenRoadNetwork(ITestOutputHelper testOutputHelper)
    : RoadNetworkTestBase(testOutputHelper)
{
    [Fact]
    public async Task WhenRoadNetworkRequest_ThenSnapshotTaken()
    {
        // Arrange
        await Given(Organizations.ToStreamName(TestData.ChangedByOrganization),
            new ImportedOrganization
            {
                Code = TestData.ChangedByOrganization,
                Name = TestData.ChangedByOrganizationName,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            });
        await Given(RoadNetworkStreamNameProvider.Default,
            new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode1Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment1Added
                    }
                ],
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            },
            new RoadNetworkChangesAccepted
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
                        RoadNodeAdded = TestData.StartNode2Added
                    },
                    new AcceptedChange
                    {
                        RoadNodeAdded = TestData.EndNode2Added
                    },
                    new AcceptedChange
                    {
                        RoadSegmentAdded = TestData.Segment2Added
                    }
                ],
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            });

        var snapshotReaderMock = new Mock<IRoadNetworkSnapshotReader>();
        var snapshotWriterMock = new Mock<IRoadNetworkSnapshotWriter>();

        // Act
        var request = new CreateRoadNetworkSnapshotRequest
        {
            StreamVersion = 3
        };
        var handler = new CreateRoadNetworkSnapshotSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            RoadRegistryContext,
            snapshotReaderMock.Object,
            snapshotWriterMock.Object,
            LoggerFactory
        );
        await handler.Handle(new CreateRoadNetworkSnapshotSqsLambdaRequest(
            RoadNetworkStreamNameProvider.Default,
            new CreateRoadNetworkSnapshotSqsRequest
            {
                Request = request,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = ObjectProvider.Create<ProvenanceData>()
            }),
            CancellationToken.None);

        // Assert
        var expectedSnapshotStreamVersion = 1;
        snapshotWriterMock.Verify(x => x.WriteSnapshot(It.IsAny<RoadNetworkSnapshot>(), expectedSnapshotStreamVersion, It.IsAny<CancellationToken>()));
    }
}
