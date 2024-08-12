namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Requests;
using RoadRegistry.Tests.Framework;
using Sqs.RoadSegments;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public sealed class SqsLambdaHandlerTests : BackOfficeLambdaTest
{
    public SqsLambdaHandlerTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task TicketShouldBeUpdatedToPendingAndCompleted()
    {
        var ticketing = new Mock<ITicketing>();
        var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

        var sqsLambdaRequest = new LinkStreetNameSqsLambdaRequest(Guid.NewGuid().ToString(), new LinkStreetNameSqsRequest
        {
            Request = new LinkStreetNameRequest(1, null, null, null),
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        });

        var sut = new FakeLambdaHandler(
            Container.Resolve<SqsLambdaHandlerOptions>(),
            new FakeRetryPolicy(),
            ticketing.Object,
            idempotentCommandHandler.Object,
            RoadRegistryContext,
            new NullLogger<FakeLambdaHandler>());

        await sut.Handle(sqsLambdaRequest, CancellationToken.None);

        ticketing.Verify(x => x.Pending(sqsLambdaRequest.TicketId, CancellationToken.None), Times.Once);
        ticketing.Verify(
            x => x.Complete(sqsLambdaRequest.TicketId,
                new TicketResult(new ETagResponse(string.Empty, string.Empty)), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task WhenIfMatchHeaderValueIsMismatch_ThenTicketingErrorIsExpected()
    {
        // Arrange
        var ticketing = new Mock<ITicketing>();

        var roadSegmentId = new RoadSegmentId(456);

        await AddMeasuredRoadSegment(roadSegmentId);

        var sut = new FakeLambdaHandler(
            Container.Resolve<SqsLambdaHandlerOptions>(),
            new FakeRetryPolicy(),
            ticketing.Object,
            MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
            RoadRegistryContext,
            new NullLogger<FakeLambdaHandler>());

        // Act
        await sut.Handle(new LinkStreetNameSqsLambdaRequest(RoadNetworkStreamNameProvider.Default, new LinkStreetNameSqsRequest
        {
            IfMatchHeaderValue = "Outdated",
            Request = new LinkStreetNameRequest(roadSegmentId, RoadSegmentGeometryDrawMethod.Measured, null, null),
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        }), CancellationToken.None);

        //Assert
        ticketing.Verify(x =>
            x.Error(
                It.IsAny<Guid>(),
                new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.", "PreconditionFailed"),
                CancellationToken.None));
    }

    [Fact]
    public async Task WhenNoIfMatchHeaderValueIsPresent_ThenInnerHandleIsCalled()
    {
        var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

        var sqsLambdaRequest = new LinkStreetNameSqsLambdaRequest(Guid.NewGuid().ToString(), new LinkStreetNameSqsRequest
        {
            Request = new LinkStreetNameRequest(0, null, null, null),
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        });

        var sut = new FakeLambdaHandler(
            Container.Resolve<SqsLambdaHandlerOptions>(),
            new FakeRetryPolicy(),
            Mock.Of<ITicketing>(),
            idempotentCommandHandler.Object,
            RoadRegistryContext,
            new NullLogger<FakeLambdaHandler>());

        await sut.Handle(sqsLambdaRequest, CancellationToken.None);

        //Assert
        idempotentCommandHandler
            .Verify(x =>
                    x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>(), new CancellationToken()),
                Times.Once);
    }

    [Fact]
    public async Task WhenRoadSegmentNotFoundException_ThenTicketingErrorIsExpected()
    {
        var ticketing = new Mock<ITicketing>();

        var sqsLambdaRequest = new LinkStreetNameSqsLambdaRequest(Guid.NewGuid().ToString(), new LinkStreetNameSqsRequest
        {
            Request = new LinkStreetNameRequest(0, null, null, null),
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>()
        });

        var sut = new FakeLambdaHandler(
            Container.Resolve<SqsLambdaHandlerOptions>(),
            new FakeRetryPolicy(),
            ticketing.Object,
            MockExceptionIdempotentCommandHandler<RoadSegmentNotFoundException>().Object,
            RoadRegistryContext,
            new NullLogger<FakeLambdaHandler>());

        await sut.Handle(sqsLambdaRequest, CancellationToken.None);

        //Assert
        ticketing.Verify(x =>
            x.Error(sqsLambdaRequest.TicketId, new TicketError("Dit wegsegment bestaat niet.", "NotFound"),
                CancellationToken.None));
        ticketing.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None),
            Times.Never);
    }
}

public sealed class FakeLambdaHandler : SqsLambdaHandler<LinkStreetNameSqsLambdaRequest>
{
    public FakeLambdaHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ILogger<FakeLambdaHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
    }

    protected override async Task<object> InnerHandle(LinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await IdempotentCommandHandler.Dispatch(
            Guid.NewGuid(),
            new object(),
            new Dictionary<string, object>(),
            cancellationToken);

        return new ETagResponse(string.Empty, string.Empty);
    }
}
