namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenCreateRoadNetworkSnapshot.Abstractions.Fixtures;

using AutoFixture;
using BackOffice;
using BackOffice.Abstractions.RoadNetworks;
using BackOffice.Configuration;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Framework;
using Handlers;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Requests;
using SqlStreamStore;
using Sqs.RoadNetworks;

public abstract class WhenCreateRoadNetworkSnapshotFixture : SqsLambdaHandlerFixture<CreateRoadNetworkSnapshotSqsLambdaRequestHandler, CreateRoadNetworkSnapshotSqsLambdaRequest, CreateRoadNetworkSnapshotSqsRequest>
{
    protected readonly Mock<IRoadNetworkSnapshotReader> SnapshotReader;
    protected readonly RoadNetworkSnapshotStrategyOptions SnapshotStrategyOptions;
    protected readonly Mock<IRoadNetworkSnapshotWriter> SnapshotWriter;

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

    protected Organisation Organisation { get; }
    protected abstract CreateRoadNetworkSnapshotRequest Request { get; }

    protected override CreateRoadNetworkSnapshotSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override CreateRoadNetworkSnapshotSqsLambdaRequest SqsLambdaRequest => new(RoadNetworkStreamNameProvider.Default, SqsRequest);

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
    protected abstract Mock<IRoadNetworkSnapshotReader> CreateSnapshotReaderMock(IRoadNetworkSnapshotReader snapshotReader);
    protected abstract Mock<IRoadNetworkSnapshotWriter> CreateSnapshotWriterMock(IRoadNetworkSnapshotWriter snapshotWriter);
}
