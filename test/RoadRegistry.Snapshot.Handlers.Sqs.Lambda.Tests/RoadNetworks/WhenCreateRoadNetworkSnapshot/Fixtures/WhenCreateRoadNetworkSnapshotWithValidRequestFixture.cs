namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenCreateRoadNetworkSnapshot.Fixtures;

using Abstractions.Fixtures;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using Moq;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Abstractions.RoadNetworks;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Hosts;
using RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Configuration;
using RoadRegistry.Tests.BackOffice.Core;
using SqlStreamStore;
using AcceptedChange = BackOffice.Messages.AcceptedChange;

public class WhenCreateRoadNetworkSnapshotWithValidRequestFixture : WhenCreateRoadNetworkSnapshotFixture
{
    public WhenCreateRoadNetworkSnapshotWithValidRequestFixture(
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

    protected override CreateRoadNetworkSnapshotRequest Request => new()
    {
        StreamVersion = 3
    };

    protected override Mock<IRoadNetworkSnapshotReader> CreateSnapshotReaderMock(IRoadNetworkSnapshotReader snapshotReader)
    {
        var mock = new Mock<IRoadNetworkSnapshotReader>();
        mock.Setup(m => m.ReadSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((new RoadNetworkSnapshot(), 0));
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
        var x = RoadNetworkTestHelpers.Create();

        await Given(Organizations.ToStreamName(x.ChangedByOrganization), new ImportedOrganization
        {
            Code = x.ChangedByOrganization,
            Name = x.ChangedByOrganizationName,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = x.RequestId,
            Reason = x.ReasonForChange,
            Operator = x.ChangedByOperator,
            OrganizationId = x.ChangedByOrganization,
            Organization = x.ChangedByOrganizationName,
            Changes = new[]
            {
                new AcceptedChange
                {
                    RoadNodeAdded = x.StartNode1Added
                },
                new AcceptedChange
                {
                    RoadNodeAdded = x.EndNode1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAdded = x.Segment1Added
                }
            },
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });
        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = x.RequestId,
            Reason = x.ReasonForChange,
            Operator = x.ChangedByOperator,
            OrganizationId = x.ChangedByOrganization,
            Organization = x.ChangedByOrganizationName,
            Changes = new[]
            {
                new AcceptedChange
                {
                    RoadNodeAdded = x.StartNode2Added
                },
                new AcceptedChange
                {
                    RoadNodeAdded = x.EndNode2Added
                },
                new AcceptedChange
                {
                    RoadSegmentAdded = x.Segment2Added
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
