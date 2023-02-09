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
using NodaTime;
using Requests;
using SqlStreamStore;
using Sqs.RoadNetworks;
using Reason = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Reason;

public abstract class WhenCreateRoadNetworkSnapshotFixture : SqsLambdaHandlerFixture<CreateRoadNetworkSnapshotSqsLambdaRequestHandler, CreateRoadNetworkSnapshotSqsLambdaRequest, CreateRoadNetworkSnapshotSqsRequest>
{
    protected readonly IRoadNetworkSnapshotReader SnapshotReader;
    protected readonly RoadNetworkSnapshotStrategyOptions SnapshotStrategyOptions;
    protected readonly IRoadNetworkSnapshotWriter SnapshotWriter;

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
        SnapshotReader = snapshotReader;
        SnapshotWriter = snapshotWriter;
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
        SnapshotReader,
        SnapshotWriter,
        SnapshotStrategyOptions,
        LoggerFactory.CreateLogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler>()
    );

    protected abstract RoadNetworkSnapshotStrategyOptions BuildSnapshotStrategyOptions();
}
