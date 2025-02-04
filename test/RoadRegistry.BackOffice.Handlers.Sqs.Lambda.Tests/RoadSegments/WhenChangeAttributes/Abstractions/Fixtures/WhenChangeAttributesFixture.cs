namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes.Abstractions.Fixtures;

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
using RoadRegistry.StreetName;
using Sqs.RoadSegments;

public abstract class WhenChangeAttributesFixture : SqsLambdaHandlerFixture<ChangeRoadSegmentAttributesSqsLambdaRequestHandler, ChangeRoadSegmentAttributesSqsLambdaRequest, ChangeRoadSegmentAttributesSqsRequest>
{
    protected readonly ApplicationMetadata ApplicationMetadata = new(RoadRegistryApplication.Lambda);

    protected WhenChangeAttributesFixture(IConfiguration configuration, ICustomRetryPolicy customRetryPolicy, IClock clock, SqsLambdaHandlerOptions options)
        : base(configuration, customRetryPolicy, clock, options)
    {
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
        EditorContext,
        new RecyclableMemoryStreamManager(),
        FileEncoding.UTF8,
        new FakeOrganizationCache(),
        new StreetNameCacheClient(new FakeStreetNameCache()
            .AddStreetName(WellKnownStreetNameIds.Proposed, "Proposed street", "voorgesteld")
            .AddStreetName(WellKnownStreetNameIds.Current, "Current street", "inGebruik")
            .AddStreetName(WellKnownStreetNameIds.Retired, "Retired street", "gehistoreerd")
            .AddStreetName(WellKnownStreetNameIds.Null, "Not found street", null)
        ),
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
                    new UseOvoCodeInChangeRoadNetworkFeatureToggle(true),
                    new FakeExtractUploadFailedEmailClient(),
                    LoggerFactory
                )
            }), ApplicationMetadata);
    }

    protected static class WellKnownStreetNameIds
    {
        public const int Proposed = 1;
        public const int Current = 2;
        public const int Retired = 3;
        public const int Null = 4;
    }
}
