﻿namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenCreatingOutline;

using Abstractions.RoadSegmentsOutline;
using Autofac;
using AutoFixture;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using FeatureToggles;
using FluentAssertions;
using Framework;
using Handlers;
using Hosts;
using Messages;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Requests;
using RoadRegistry.Tests.BackOffice;
using Sqs.RoadSegments;
using Xunit.Abstractions;
using LineString = NetTopologySuite.Geometries.LineString;

public class GivenOrganizationExists: BackOfficeLambdaTest
{
    public ICustomRetryPolicy CustomRetryPolicy { get; }

    public GivenOrganizationExists(
        ITestOutputHelper testOutputHelper,
        ICustomRetryPolicy customRetryPolicy)
        : base(testOutputHelper)
    {
        CustomRetryPolicy = customRetryPolicy;

        ObjectProvider.CustomizeRoadSegmentOutline();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();

        ObjectProvider.Customize<LineString>(customization =>
            customization.FromFactory(_ => new LineString(
                new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(10, 0, 10)]),
                GeometryConfiguration.GeometryFactory)
            ).OmitAutoProperties()
        );
    }

    [Fact]
    public async Task WhenValidRequest_ThenChangeRoadNetwork()
    {
        // Arrange
        await GivenOrganization();

        // Act
        var request = ObjectProvider.Create<CreateRoadSegmentOutlineRequest>();
        await HandleRequest(request);

        // Assert
        await ThrowIfLastCommandIsRoadNetworkChangesRejected();

        var roadSegmentId = new RoadSegmentId(1);
        await VerifyThatTicketHasCompleted(roadSegmentId);

        var command = await Store.GetLastCommand<RoadNetworkChangesAccepted>();
        command.Changes.Length.Should().Be(1);
        command.Changes.Single().RoadSegmentAdded.Id.Should().Be(roadSegmentId);
    }

    [Fact]
    public async Task WhenStatusIsUnknown_ThenTicketError()
    {
        // Arrange
        await GivenOrganization();

        // Act
        var request = ObjectProvider.Create<CreateRoadSegmentOutlineRequest>() with
        {
            Status = RoadSegmentStatus.Unknown
        };
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError("WegsegmentStatusNietCorrect", "Wegsegment status is foutief. 'Unknown' is geen geldige waarde.");
    }

    [Fact]
    public async Task WhenMorphologyIsUnknown_ThenTicketError()
    {
        // Arrange
        await GivenOrganization();

        // Act
        var request = ObjectProvider.Create<CreateRoadSegmentOutlineRequest>() with
        {
            Morphology = RoadSegmentMorphology.Unknown
        };
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError("MorfologischeWegklasseNietCorrect", "Morfologische wegklasse is foutief. 'Unknown' is geen geldige waarde.");
    }

    private async Task HandleRequest(CreateRoadSegmentOutlineRequest request)
    {
        var sqsRequest = new CreateRoadSegmentOutlineSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        };

        var sqsLambdaRequest = new CreateRoadSegmentOutlineSqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        var handler = new CreateRoadSegmentOutlineSqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            CustomRetryPolicy,
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            ScopedContainer.Resolve<IChangeRoadNetworkDispatcher>(),
            OrganizationCache,
            new NullLogger<CreateRoadSegmentOutlineSqsLambdaRequestHandler>()
        );
        await handler.Handle(sqsLambdaRequest, CancellationToken.None);
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
                    new UseOvoCodeInChangeRoadNetworkFeatureToggle(true),
                    new FakeExtractUploadFailedEmailClient(),
                    new RoadNetworkEventWriter(Store, EnrichEvent.WithTime(Clock)),
                    LoggerFactory
                )
            ]), ApplicationMetadata))
            .SingleInstance();
    }
}
