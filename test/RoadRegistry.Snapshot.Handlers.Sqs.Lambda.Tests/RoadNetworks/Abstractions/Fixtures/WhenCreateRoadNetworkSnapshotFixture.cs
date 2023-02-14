namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.Abstractions.Fixtures;

using AutoFixture;
using BackOffice;
using BackOffice.Abstractions.RoadNetworks;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Configuration;
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
        Organisation = Organisation.DigitaalVlaanderen;
        SnapshotReader = CreateSnapshotReaderMock(snapshotReader);
        SnapshotWriter = CreateSnapshotWriterMock(snapshotWriter);
        SnapshotStrategyOptions = BuildSnapshotStrategyOptions();

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
        Store,
        SnapshotStrategyOptions,
        LoggerFactory.CreateLogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler>()
    );

    protected abstract RoadNetworkSnapshotStrategyOptions BuildSnapshotStrategyOptions();
}
