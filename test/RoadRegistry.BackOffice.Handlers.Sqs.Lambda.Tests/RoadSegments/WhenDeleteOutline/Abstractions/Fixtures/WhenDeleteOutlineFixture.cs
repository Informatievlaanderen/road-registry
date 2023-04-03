namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline.Abstractions.Fixtures;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;
using RoadRegistry.Hosts;
using RoadRegistry.Tests.BackOffice;

public abstract class WhenDeleteOutlineFixture : SqsLambdaHandlerFixture<DeleteRoadSegmentOutlineSqsLambdaRequestHandler, DeleteRoadSegmentOutlineSqsLambdaRequest, DeleteRoadSegmentOutlineSqsRequest>
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected WhenDeleteOutlineFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        ObjectProvider.CustomizeRoadSegmentOutlineStatus();
        ObjectProvider.CustomizeRoadSegmentOutlineSurfaceType();
        ObjectProvider.CustomizeRoadSegmentOutlineWidth();
        ObjectProvider.CustomizeRoadSegmentOutlineLaneCount();
        ObjectProvider.CustomizeRoadSegmentOutlineMorphology();

        Organisation = ObjectProvider.Create<Organisation>();
    }

    protected Organisation Organisation { get; }
    protected abstract DeleteRoadSegmentOutlineRequest Request { get; }

    protected override DeleteRoadSegmentOutlineSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override DeleteRoadSegmentOutlineSqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

    protected override DeleteRoadSegmentOutlineSqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Options,
        CustomRetryPolicy,
        TicketingMock.Object,
        IdempotentCommandHandler,
        RoadRegistryContext,
        ChangeRoadNetworkDispatcher,
        new FakeDistributedStreamStoreLockOptions(),
        new NullLogger<DeleteRoadSegmentOutlineSqsLambdaRequestHandler>()
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
