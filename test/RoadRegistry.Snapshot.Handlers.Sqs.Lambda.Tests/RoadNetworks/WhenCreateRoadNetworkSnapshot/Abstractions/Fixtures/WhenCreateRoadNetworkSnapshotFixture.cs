namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenCreateRoadNetworkSnapshot.Abstractions.Fixtures;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Abstractions.RoadNetworks;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Hosts;
using RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Configuration;
using RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Handlers;
using RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Requests;
using RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.Snapshot.Handlers.Sqs.RoadNetworks;
using SqlStreamStore;
using Reason = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Reason;

public abstract class WhenCreateRoadNetworkSnapshotFixture : SqsLambdaHandlerFixture<CreateRoadNetworkSnapshotSqsLambdaRequestHandler, CreateRoadNetworkSnapshotSqsLambdaRequest, CreateRoadNetworkSnapshotSqsRequest>
{
    protected readonly Mock<IRoadNetworkSnapshotReader> SnapshotReader;
    protected readonly Mock<IRoadNetworkSnapshotWriter> SnapshotWriter;
    protected readonly RoadNetworkSnapshotStrategyOptions SnapshotStrategyOptions;

    protected WhenCreateRoadNetworkSnapshotFixture(
        IConfiguration configuration,
        ICustomRetryPolicy customRetryPolicy,
        IStreamStore streamStore,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IClock clock,
        SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, streamStore, roadNetworkCommandQueue, clock, options)
    {
        Organisation = ObjectProvider.Create<Organisation>();
        SnapshotReader = CreateSnapshotReaderMock(snapshotReader);
        SnapshotWriter = CreateSnapshotWriterMock(snapshotWriter);
        SnapshotStrategyOptions = BuildSnapshotStrategyOptions();
    }

    protected abstract Mock<IRoadNetworkSnapshotReader> CreateSnapshotReaderMock(IRoadNetworkSnapshotReader snapshotReader);
    protected abstract Mock<IRoadNetworkSnapshotWriter> CreateSnapshotWriterMock(IRoadNetworkSnapshotWriter snapshotWriter);

    protected Organisation Organisation { get; }
    protected abstract CreateRoadNetworkSnapshotRequest Request { get; }

    protected override CreateRoadNetworkSnapshotSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override CreateRoadNetworkSnapshotSqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

    protected override CreateRoadNetworkSnapshotSqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Options,
        CustomRetryPolicy,
        TicketingMock.Object,
        RoadRegistryContext,
        SnapshotReader.Object,
        SnapshotWriter.Object,
        SnapshotStrategyOptions,
        LoggerFactory.CreateLogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler>()
    );

    protected abstract RoadNetworkSnapshotStrategyOptions BuildSnapshotStrategyOptions();
}