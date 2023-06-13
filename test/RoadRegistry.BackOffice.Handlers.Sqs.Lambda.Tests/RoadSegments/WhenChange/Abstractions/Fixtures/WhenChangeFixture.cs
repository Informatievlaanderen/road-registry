namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChange.Abstractions.Fixtures;

using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Framework;
using Handlers;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using Requests;
using RoadRegistry.Tests.BackOffice;
using Sqs.RoadSegments;

public abstract class WhenChangeFixture : SqsLambdaHandlerFixture<ChangeRoadSegmentsSqsLambdaRequestHandler, ChangeRoadSegmentsSqsLambdaRequest, ChangeRoadSegmentsSqsRequest>
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected WhenChangeFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        ObjectProvider.CustomizeRoadSegmentPositionAttributesBuilder();
        ObjectProvider.CustomizeRoadSegmentLaneCount();
        ObjectProvider.CustomizeRoadSegmentLaneDirection();
        ObjectProvider.CustomizeRoadSegmentLaneAttribute();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
        ObjectProvider.CustomizeRoadSegmentSurfaceAttribute();
        ObjectProvider.CustomizeRoadSegmentWidth();
        ObjectProvider.CustomizeRoadSegmentWidthAttribute();

        Organisation = ObjectProvider.Create<Organisation>();
    }

    protected Organisation Organisation { get; }
    protected abstract ChangeRoadSegmentsRequest Request { get; }

    protected override ChangeRoadSegmentsSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override ChangeRoadSegmentsSqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

    protected override ChangeRoadSegmentsSqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Options,
        CustomRetryPolicy,
        TicketingMock.Object,
        IdempotentCommandHandler,
        RoadRegistryContext,
        ChangeRoadNetworkDispatcher,
        new FakeDistributedStreamStoreLockOptions(),
        new NullLogger<ChangeRoadSegmentsSqsLambdaRequestHandler>()
    );

    protected override CommandHandlerDispatcher BuildCommandHandlerDispatcher()
    {
        return Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkCommandModule(
                    Store,
                    LifetimeScope,
                    new FakeRoadNetworkSnapshotReader(),
                    Clock,
                    LoggerFactory
                )
            }), ApplicationMetadata);
    }
}
