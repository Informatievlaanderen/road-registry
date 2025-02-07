namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeDynamicAttributes.Abstractions.Fixtures;

using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using FeatureToggles;
using Framework;
using Handlers;
using Hosts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using NodaTime;
using Requests;
using RoadRegistry.Tests.BackOffice;
using Sqs.RoadSegments;

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

    protected virtual ChangeRoadSegmentsDynamicAttributesSqsRequest SqsRequest => new()
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
                    LoggerFactory
                )
            }), ApplicationMetadata);
    }
}
