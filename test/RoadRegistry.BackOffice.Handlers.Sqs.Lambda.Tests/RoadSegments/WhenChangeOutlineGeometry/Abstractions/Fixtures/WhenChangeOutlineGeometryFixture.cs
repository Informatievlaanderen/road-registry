namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeOutlineGeometry.Abstractions.Fixtures;

using AutoFixture;
using BackOffice.Abstractions.RoadSegmentsOutline;
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

public abstract class WhenChangeOutlineGeometryFixture : SqsLambdaHandlerFixture<ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler, ChangeRoadSegmentOutlineGeometrySqsLambdaRequest, ChangeRoadSegmentOutlineGeometrySqsRequest>
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected WhenChangeOutlineGeometryFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
        Organisation = ObjectProvider.Create<Organisation>();
    }

    protected override void CustomizeTestData(Fixture fixture)
    {
        fixture.CustomizeRoadSegmentOutlineGeometryDrawMethod();
    }

    protected Organisation Organisation { get; }
    
    protected abstract ChangeRoadSegmentOutlineGeometryRequest Request { get; }

    protected override ChangeRoadSegmentOutlineGeometrySqsRequest SqsRequest => new()
    {
        Request = Request,
        TicketId = Guid.NewGuid(),
        Metadata = new Dictionary<string, object?>(),
        ProvenanceData = ObjectProvider.Create<ProvenanceData>()
    };

    protected override ChangeRoadSegmentOutlineGeometrySqsLambdaRequest SqsLambdaRequest => new(RoadNetwork.Identifier.ToString(), SqsRequest);

    protected override ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler SqsLambdaRequestHandler => new(
        Options,
        CustomRetryPolicy,
        TicketingMock.Object,
        IdempotentCommandHandler,
        RoadRegistryContext,
        ChangeRoadNetworkDispatcher,
        new NullLogger<ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler>()
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
