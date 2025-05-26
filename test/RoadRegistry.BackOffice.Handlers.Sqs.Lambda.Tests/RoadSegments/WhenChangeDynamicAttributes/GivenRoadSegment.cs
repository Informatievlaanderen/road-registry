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
                        FromPosition = x.From,
                        ToPosition = index != lanePositions.Length - 1 ? x.To : lastToPosition,
                        Count = ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Direction = ObjectProvider.Create<RoadSegmentLaneDirection>()
                    }).ToArray();

                var surfacePositions = ObjectProvider.Create<Func<double, RoadSegmentPositionAttribute[]>>()(geometryLength);
                change.Surfaces = surfacePositions
                    .Select((x, index) => new ChangeRoadSegmentSurfaceAttributeRequest
                    {
                        FromPosition = x.From,
                        ToPosition = index != surfacePositions.Length - 1 ? x.To : lastToPosition,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>()
                    }).ToArray();

                var widthPositions = ObjectProvider.Create<Func<double, RoadSegmentPositionAttribute[]>>()(geometryLength);
                change.Widths = widthPositions
                    .Select((x, index) => new ChangeRoadSegmentWidthAttributeRequest
                    {
                        FromPosition = x.From,
                        ToPosition = index != lanePositions.Length - 1 ? x.To : lastToPosition,
                        Width = ObjectProvider.Create<RoadSegmentWidth>()
                    }).ToArray();
            });

        await Given(Organizations.ToStreamName(new OrganizationId(OrganizationDbaseRecord.ORG.Value)), new ImportedOrganization
        {
            Code = OrganizationDbaseRecord.ORG.Value,
            Name = OrganizationDbaseRecord.ORG.Value,
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        await Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = TestData.RequestId,
            Reason = TestData.ReasonForChange,
            Operator = TestData.ChangedByOperator,
            OrganizationId = TestData.ChangedByOrganization,
            Organization = TestData.ChangedByOrganizationName,
            Changes =
            [
                new AcceptedChange
                {
                    RoadSegmentAdded = TestData.Segment1Added
                }
            ],
            When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
        });

        await EditorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, Clock));
        await EditorContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasCompleted(new ChangeRoadSegmentsDynamicAttributesResponse());

        var roadSegmentId = new RoadSegmentId(TestData.Segment1Added.Id);

        var command = await Store.GetLastMessage<RoadNetworkChangesAccepted>();
        var @event = command.Changes.Single().RoadSegmentAttributesModified;
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
