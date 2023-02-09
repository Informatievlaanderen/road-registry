namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.Fixtures;

using Abstractions.Fixtures;
using BackOffice;
using BackOffice.Abstractions.RoadNetworks;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Configuration;
using Hosts;
using Microsoft.Extensions.Configuration;
using NodaTime;
using SqlStreamStore;

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

    protected override CreateRoadNetworkSnapshotRequest Request => new();

    protected override RoadNetworkSnapshotStrategyOptions BuildSnapshotStrategyOptions()
    {
        return new() { EventCount = 3 };
    }

    protected override Task SetupAsync()
    {
        throw new NotImplementedException();
    }

    protected override Task<bool> VerifyTicketAsync()
    {
        throw new NotImplementedException();
    }
}
