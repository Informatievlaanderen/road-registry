namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangingOutlineGeometry;

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
using GeometryTranslator = GeometryTranslator;
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

        ObjectProvider.CustomizeRoadSegmentOutlineGeometryDrawMethod();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(99999.99)]
    public async Task WhenValidRequest_ThenChangeRoadNetwork(double length)
    {
        // Arrange
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        // Act
        var lineString = new LineString(
            new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(length, 0, length)]),
            GeometryConfiguration.GeometryFactory);

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(lineString.ToMultiLineString())
        );
        await HandleRequest(request);

        // Assert
        await ThrowIfLastCommandIsRoadNetworkChangesRejected();

        await VerifyThatTicketHasCompleted(roadSegmentId);

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        command.Changes.Length.Should().Be(1);
        command.Changes.Single().RoadSegmentGeometryModified.Id.Should().Be(roadSegmentId);
    }

    [Fact]
    public async Task WhenGeometryIsTooLong_ThenTicketError()
    {
        // Arrange
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        // Act
        var tooLongLineString = new LineString(
            new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(100000, 0, 100000)]),
            GeometryConfiguration.GeometryFactory);

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(tooLongLineString.ToMultiLineString())
        );
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasErrorList("MiddellijnGeometrieTeLang", "De opgegeven geometrie van wegsegment met id 1 zijn lengte is groter of gelijk dan 100000 meter.");
    }

    [Fact]
    public async Task WhenGeometryIsTooShort_ThenTicketError()
    {
        // Arrange
        await GivenOrganization();

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);
        await AddOutlinedRoadSegment(roadSegmentId);

        // Act
        var tooLongLineString = new LineString(
            new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(1.99, 0, 1.99)]),
            GeometryConfiguration.GeometryFactory);

        var request = new ChangeRoadSegmentOutlineGeometryRequest(
            roadSegmentId,
            GeometryTranslator.Translate(tooLongLineString.ToMultiLineString())
        );
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasErrorList("MiddellijnGeometrieKorterDanMinimum", "De opgegeven geometrie van wegsegment met id 1 heeft niet de minimale lengte van 2 meter.");
    }

    private async Task HandleRequest(ChangeRoadSegmentOutlineGeometryRequest request)
    {
        var sqsRequest = new ChangeRoadSegmentOutlineGeometrySqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        };

        var sqsLambdaRequest = new ChangeRoadSegmentOutlineGeometrySqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        var handler = new ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            CustomRetryPolicy,
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            ScopedContainer.Resolve<IChangeRoadNetworkDispatcher>(),
            new NullLogger<ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler>()
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
