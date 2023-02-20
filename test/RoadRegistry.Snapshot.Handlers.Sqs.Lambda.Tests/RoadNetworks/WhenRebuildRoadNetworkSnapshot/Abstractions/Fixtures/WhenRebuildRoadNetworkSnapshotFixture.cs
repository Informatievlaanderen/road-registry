namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenRebuildRoadNetworkSnapshot.Abstractions.Fixtures;

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

public abstract class WhenRebuildRoadNetworkSnapshotFixture : SqsLambdaHandlerFixture<RebuildRoadNetworkSnapshotSqsLambdaRequestHandler, RebuildRoadNetworkSnapshotSqsLambdaRequest, RebuildRoadNetworkSnapshotSqsRequest>
{
    protected readonly Mock<IRoadNetworkSnapshotReader> SnapshotReader;
    protected readonly Mock<IRoadNetworkSnapshotWriter> SnapshotWriter;

    protected WhenRebuildRoadNetworkSnapshotFixture(
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
        Organisation = Organisation.DigitaalVlaanderen;
        SnapshotReader = CreateSnapshotReaderMock(snapshotReader);
        SnapshotWriter = CreateSnapshotWriterMock(snapshotWriter);

        ObjectProvider.Customize<ProvenanceData>(customization =>
            customization.FromSeed(generator => new ProvenanceData(new Provenance(Clock.GetCurrentInstant(),
                Application.RoadRegistry,
                new Reason("TEST"),
                new Operator("TEST"),
                Modification.Unknown,
                Organisation)))
        );
    }

    protected abstract Mock<IRoadNetworkSnapshotReader> CreateSnapshotReaderMock(IRoadNetworkSnapshotReader snapshotReader);
    protected abstract Mock<IRoadNetworkSnapshotWriter> CreateSnapshotWriterMock(IRoadNetworkSnapshotWriter snapshotWriter);

    protected Organisation Organisation { get; }
    protected abstract RebuildRoadNetworkSnapshotRequest Request { get; }

    protected override RebuildRoadNetworkSnapshotSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override RebuildRoadNetworkSnapshotSqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

    protected override RebuildRoadNetworkSnapshotSqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Options,
        CustomRetryPolicy,
        TicketingMock.Object,
        RoadRegistryContext,
        SnapshotReader.Object,
        SnapshotWriter.Object,
        LoggerFactory.CreateLogger<RebuildRoadNetworkSnapshotSqsLambdaRequestHandler>()
    );
}
