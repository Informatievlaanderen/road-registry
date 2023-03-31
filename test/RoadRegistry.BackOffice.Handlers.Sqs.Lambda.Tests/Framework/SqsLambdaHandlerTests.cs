namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Hosts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.Tests.Framework;
using Sqs.RoadSegments;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public sealed class SqsLambdaHandlerTests : BackOfficeLambdaTest
{
    public SqsLambdaHandlerTests(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory) : base(testOutputHelper, loggerFactory)
    {
    }

    [Fact]
    public async Task TicketShouldBeUpdatedToPendingAndCompleted()
    {
        var ticketing = new Mock<ITicketing>();
        var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

        var sqsLambdaRequest = new LinkStreetNameSqsLambdaRequest(Guid.NewGuid().ToString(), new LinkStreetNameSqsRequest
        {
            Request = new LinkStreetNameRequest(1, null, null),
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
                new TicketResult(new ETagResponse("bla", "etag")), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task WhenIfMatchHeaderValueIsMismatch_ThenTicketingErrorIsExpected()
    {
        // Arrange
        var ticketing = new Mock<ITicketing>();

        var roadSegmentId = new RoadSegmentId(456);

        await AddRoadSegment(roadSegmentId);

        var sut = new FakeLambdaHandler(
            Container.Resolve<SqsLambdaHandlerOptions>(),
            new FakeRetryPolicy(),
            ticketing.Object,
            MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
            RoadRegistryContext,
            new NullLogger<FakeLambdaHandler>());

        // Act
        await sut.Handle(new LinkStreetNameSqsLambdaRequest(RoadNetwork.Identifier.ToString(), new LinkStreetNameSqsRequest
        {
            IfMatchHeaderValue = "Outdated",
            Request = new LinkStreetNameRequest(roadSegmentId, null, null),
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
            Request = new LinkStreetNameRequest(0, null, null),
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
            Request = new LinkStreetNameRequest(0, null, null),
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
            x.Error(sqsLambdaRequest.TicketId, new TicketError("Onbestaand wegsegment.", "NotFound"),
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

    protected override Task<ETagResponse> InnerHandleAsync(LinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        IdempotentCommandHandler.Dispatch(
            Guid.NewGuid(),
            new object(),
            new Dictionary<string, object>(),
            cancellationToken);

        return Task.FromResult(new ETagResponse("bla", "etag"));
    }
}
