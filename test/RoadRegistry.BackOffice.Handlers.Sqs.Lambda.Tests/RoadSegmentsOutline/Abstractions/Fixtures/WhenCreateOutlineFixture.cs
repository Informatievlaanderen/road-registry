namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.Abstractions.Fixtures;

using AutoFixture;
using BackOffice.Abstractions.RoadSegmentsOutline;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Framework;
using Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using Requests;
using RoadRegistry.Tests.BackOffice;
using SqlStreamStore;
using Sqs.RoadSegments;

public abstract class WhenCreateOutlineFixture : SqsLambdaHandlerFixture<CreateRoadSegmentOutlineSqsLambdaRequestHandler, CreateRoadSegmentOutlineSqsLambdaRequest, CreateRoadSegmentOutlineSqsRequest>
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected WhenCreateOutlineFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IStreamStore streamStore, IRoadNetworkCommandQueue roadNetworkCommandQueue, IClock clock)
        : base(configuration, customRetryPolicy, streamStore, roadNetworkCommandQueue, clock)
    {
        Organisation = Organisation.DigitaalVlaanderen;

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
    protected abstract CreateRoadSegmentOutlineRequest Request { get; }

    protected override CreateRoadSegmentOutlineSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override CreateRoadSegmentOutlineSqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

    protected override CreateRoadSegmentOutlineSqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Options,
        CustomRetryPolicy,
        TicketingMock.Object,
        IdempotentCommandHandler,
        RoadRegistryContext,
        RoadNetworkCommandQueue,
        new NullLogger<CreateRoadSegmentOutlineSqsLambdaRequestHandler>()
    );

    protected override CommandHandlerDispatcher BuildCommandHandlerDispatcher()
    {
        return Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkCommandModule(
                    Store,
                    EntityMapFactory,
                    new FakeRoadNetworkSnapshotReader(),
                    new FakeRoadNetworkSnapshotWriter(),
                    Clock,
                    LoggerFactory
                )
            }), ApplicationMetadata);
    }
}
