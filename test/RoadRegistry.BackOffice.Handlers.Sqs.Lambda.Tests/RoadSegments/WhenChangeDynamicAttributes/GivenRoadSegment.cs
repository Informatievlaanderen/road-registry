namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeDynamicAttributes;

using Abstractions.RoadSegments;
using Autofac;
using AutoFixture;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Core;
using FeatureToggles;
using FluentAssertions;
using Framework;
using Handlers;
using Hosts;
using Messages;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using NodaTime.Text;
using Requests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.Tests.Framework;
using Sqs.RoadSegments;
using Xunit.Abstractions;
using AcceptedChange = Messages.AcceptedChange;

public class GivenRoadSegment : BackOfficeLambdaTest
{
    public GivenRoadSegment(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task WithValidRequest_ThenSucceeded()
    {
        // Arrange
        var request = new ChangeRoadSegmentsDynamicAttributesRequest()
            .Add(new RoadSegmentId(TestData.Segment1Added.Id), change =>
            {
                var geometryLength = GeometryTranslator.Translate(TestData.Segment1Added.Geometry).Length;
                var lastToPosition = RoadSegmentPosition.FromDouble(Math.Round(geometryLength, 3));

                var lanePositions = ObjectProvider.Create<Func<double, RoadSegmentPositionAttribute[]>>()(geometryLength);
                change.Lanes = lanePositions
                    .Select((x, index) => new ChangeRoadSegmentLaneAttributeRequest
                    {
                        FromPosition = RoadSegmentPosition.FromDouble(index),
                        ToPosition = index != lanePositions.Length - 1 ? RoadSegmentPosition.FromDouble(index + 1) : lastToPosition,
                        Count = ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Direction = ObjectProvider.Create<RoadSegmentLaneDirection>()
                    }).ToArray();

                var surfacePositions = ObjectProvider.Create<Func<double, RoadSegmentPositionAttribute[]>>()(geometryLength);
                change.Surfaces = surfacePositions
                    .Select((x, index) => new ChangeRoadSegmentSurfaceAttributeRequest
                    {
                        FromPosition = RoadSegmentPosition.FromDouble(index),
                        ToPosition = index != surfacePositions.Length - 1 ? RoadSegmentPosition.FromDouble(index + 1) : lastToPosition,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>()
                    }).ToArray();

                var widthPositions = ObjectProvider.Create<Func<double, RoadSegmentPositionAttribute[]>>()(geometryLength);
                change.Widths = widthPositions
                    .Select((x, index) => new ChangeRoadSegmentWidthAttributeRequest
                    {
                        FromPosition = RoadSegmentPosition.FromDouble(index),
                        ToPosition = index != widthPositions.Length - 1 ? RoadSegmentPosition.FromDouble(index + 1) : lastToPosition,
                        Width = ObjectProvider.Create<RoadSegmentWidth>()
                    }).ToArray();
            });

        await Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization);

        await Given(RoadNetworks.Stream, new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadNodeAdded(TestData.StartNode1Added)
            .WithRoadNodeAdded(TestData.EndNode1Added)
            .WithRoadSegmentAdded(TestData.Segment1Added)
            .Build());

        await EditorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, Clock));
        await EditorContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasCompleted(new ChangeRoadSegmentsDynamicAttributesResponse());

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        var @event = command.Changes.Single().RoadSegmentModified;
        var change = request.ChangeRequests.Single();

        @event.Id.Should().Be(roadSegmentId);

        @event.Lanes.EqualsCollection(change.Lanes, (x1, x2) =>
            x1.FromPosition == x2.FromPosition
            && x1.ToPosition == x2.ToPosition
            && x1.Count == x2.Count
            && x1.Direction == x2.Direction
        ).Should().BeTrue();
        @event.Surfaces.EqualsCollection(change.Surfaces, (x1, x2) =>
            x1.FromPosition == x2.FromPosition
            && x1.ToPosition == x2.ToPosition
            && x1.Type == x2.Type
        ).Should().BeTrue();
        @event.Widths.EqualsCollection(change.Widths, (x1, x2) =>
            x1.FromPosition == x2.FromPosition
            && x1.ToPosition == x2.ToPosition
            && x1.Width == x2.Width
        ).Should().BeTrue();
    }

    [Fact]
    public async Task WithUnknownRoadSegmentId_ThenError()
    {
        // Arrange
        var roadSegmentId = int.MaxValue;
        var request = new ChangeRoadSegmentsDynamicAttributesRequest()
            .Add(new RoadSegmentId(roadSegmentId), _ => { });

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError(
            "NotFound",
            $"Het wegsegment met id {roadSegmentId} bestaat niet.",
            new Dictionary<string, object>
            {
                { "WegsegmentId", roadSegmentId }
            });
    }

    private async Task HandleRequest(ChangeRoadSegmentsDynamicAttributesRequest request)
    {
        var sqsRequest = new ChangeRoadSegmentsDynamicAttributesSqsRequest
        {
            Request = request,
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        };

        var sqsLambdaRequest = new ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest(RoadNetwork.Identifier.ToString(), sqsRequest);

        var handler = new ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            RoadRegistryContext,
            ScopedContainer.Resolve<IChangeRoadNetworkDispatcher>(),
            EditorContext,
            new RecyclableMemoryStreamManager(),
            FileEncoding.UTF8,
            new FakeDistributedStreamStoreLockOptions(),
            new NullLogger<ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler>()
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
                    new FakeExtractUploadFailedEmailClient(),
                    LoggerFactory
                )
            ]), ApplicationMetadata))
            .SingleInstance();
    }
}
