namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeDynamicAttributes.Abstractions.Fixtures;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using NodaTime;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Tests.BackOffice;

public abstract class WhenChangeDynamicAttributesFixture : SqsLambdaHandlerFixture<ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler, ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest, ChangeRoadSegmentsDynamicAttributesSqsRequest>
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected WhenChangeDynamicAttributesFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
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
    protected abstract ChangeRoadSegmentsDynamicAttributesRequest Request { get; }

    protected override ChangeRoadSegmentsDynamicAttributesSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

    protected override ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Options,
        CustomRetryPolicy,
        TicketingMock.Object,
        IdempotentCommandHandler,
        RoadRegistryContext,
        ChangeRoadNetworkDispatcher,
        EditorContext,
        new RecyclableMemoryStreamManager(),
        FileEncoding.UTF8,
        new FakeDistributedStreamStoreLockOptions(),
        new UseDefaultRoadNetworkFallbackForOutlinedRoadSegmentsFeatureToggle(false),
        new NullLogger<ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler>()
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
                    new RoadNetworkCommandQueue(Store, ApplicationMetadata),
                    LoggerFactory
                )
            }), ApplicationMetadata);
    }
}
