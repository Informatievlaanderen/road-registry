namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline;

using Autofac;
using AutoFixture;
using BackOffice.Abstractions.RoadSegmentsOutline;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Core;
using FeatureToggles;
using Framework;
using Handlers;
using Hosts;
using Microsoft.Extensions.Logging.Abstractions;
using Requests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using Xunit.Abstractions;

public abstract class WhenDeleteOutlineTestBase : BackOfficeLambdaTest
{
    protected WhenDeleteOutlineTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        ObjectProvider.CustomizeRoadSegmentOutline();
    }

    protected async Task HandleRequest(DeleteRoadSegmentOutlineRequest request)
    {
        var sqsRequest = new DeleteRoadSegmentOutlineSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        };

        var sqsLambdaRequest = new DeleteRoadSegmentOutlineSqsLambdaRequest(RoadNetwork.Identifier.ToString(), sqsRequest);

        var sqsLambdaRequestHandler = new DeleteRoadSegmentOutlineSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            ScopedContainer.Resolve<IChangeRoadNetworkDispatcher>(),
            new NullLogger<DeleteRoadSegmentOutlineSqsLambdaRequestHandler>()
        );

        await sqsLambdaRequestHandler.Handle(sqsLambdaRequest, CancellationToken.None);
    }

    protected override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);

        containerBuilder
            .Register(_ => Dispatch.Using(Resolve.WhenEqualToMessage(
            [
                new RoadNetworkCommandModule(
                    Store,
                    ScopedContainer,
                    new FakeRoadNetworkSnapshotReader(),
                    Clock,
                    new FakeExtractUploadFailedEmailClient(),
                    LoggerFactory
                )
            ]), ApplicationMetadata))
            .SingleInstance();
    }
}
