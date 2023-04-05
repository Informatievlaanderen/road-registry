namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes.Abstractions.Fixtures;

using AutoFixture;
using BackOffice.Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;
using RoadRegistry.Hosts;
using RoadRegistry.Tests.BackOffice;

public abstract class WhenChangeAttributesFixture : SqsLambdaHandlerFixture<ChangeRoadSegmentAttributesSqsLambdaRequestHandler, ChangeRoadSegmentAttributesSqsLambdaRequest, ChangeRoadSegmentAttributesSqsRequest>
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected WhenChangeAttributesFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
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
    
    protected abstract ChangeRoadSegmentAttributesRequest Request { get; }

    protected override ChangeRoadSegmentAttributesSqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override ChangeRoadSegmentAttributesSqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

    protected override ChangeRoadSegmentAttributesSqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Options,
        CustomRetryPolicy,
        TicketingMock.Object,
        IdempotentCommandHandler,
        RoadRegistryContext,
        ChangeRoadNetworkDispatcher,
        new NullLogger<ChangeRoadSegmentAttributesSqsLambdaRequestHandler>()
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