namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenRebuildRoadNetworkSnapshot.Fixtures;

using Abstractions.Fixtures;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Configuration;
using Microsoft.Extensions.Configuration;
using Moq;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Abstractions.RoadNetworks;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Hosts;
using RoadRegistry.Tests.BackOffice.Core;
using SqlStreamStore;
using AcceptedChange = BackOffice.Messages.AcceptedChange;

public class WhenRebuildRoadNetworkSnapshotWithValidRequestFixture : WhenRebuildRoadNetworkSnapshotFixture
{
    public WhenRebuildRoadNetworkSnapshotWithValidRequestFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IStreamStore streamStore,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, streamStore, roadNetworkCommandQueue, snapshotReader, snapshotWriter, clock, options)
    {
    }

    protected override RebuildRoadNetworkSnapshotRequest Request => new();

    protected override Mock<IRoadNetworkSnapshotReader> CreateSnapshotReaderMock(IRoadNetworkSnapshotReader snapshotReader)
    {
        var mock = new Mock<IRoadNetworkSnapshotReader>();
        mock.Setup(m => m.ReadSnapshotVersionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        return mock;
    }

    protected override Mock<IRoadNetworkSnapshotWriter> CreateSnapshotWriterMock(IRoadNetworkSnapshotWriter snapshotWriter)
    {
        var mock = new Mock<IRoadNetworkSnapshotWriter>();
        return mock;
    }
    
    protected override RoadNetworkSnapshotStrategyOptions BuildSnapshotStrategyOptions()
    {
        return new() { EventCount = 3 };
    }

    protected override async Task SetupAsync()
    {
        await Given(Organizations.ToStreamName(TestData.ChangedByOrganization), new ImportedOrganization
        {
            Code = TestData.ChangedByOrganization,
            Name = TestData.ChangedByOrganizationName,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            Changes = new[]
            {
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
            },
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            Changes = new[]
            {
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
            },
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
    }

    protected override Task<bool> VerifyTicketAsync()
    {
        return Task.FromResult(true);
    }
}
