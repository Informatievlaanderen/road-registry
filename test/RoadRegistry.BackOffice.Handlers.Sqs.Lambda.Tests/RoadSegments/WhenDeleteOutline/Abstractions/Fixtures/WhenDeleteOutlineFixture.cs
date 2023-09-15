namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline.Abstractions.Fixtures;

using AutoFixture;
using BackOffice.Abstractions.RoadSegmentsOutline;
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
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Tests.BackOffice;
using Sqs.RoadSegments;

public abstract class WhenDeleteOutlineFixture : SqsLambdaHandlerFixture<DeleteRoadSegmentOutlineSqsLambdaRequestHandler, DeleteRoadSegmentOutlineSqsLambdaRequest, DeleteRoadSegmentOutlineSqsRequest>
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected WhenDeleteOutlineFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        ObjectProvider.CustomizeRoadSegmentOutlineStatus();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
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
                    new UseOvoCodeInChangeRoadNetworkFeatureToggle(true),
                    new FakeExtractUploadFailedEmailClient(),
                    LoggerFactory
                )
            }), ApplicationMetadata);
    }
}
