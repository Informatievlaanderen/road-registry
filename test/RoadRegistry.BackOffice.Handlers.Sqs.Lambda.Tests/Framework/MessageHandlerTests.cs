namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Abstractions;
using Abstractions.RoadSegments;
using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using MediatR;
using Moq;
using Requests;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Sqs.RoadSegments;
using Xunit.Abstractions;

public sealed class MessageHandlerTests : RoadRegistryFixture
{
    public MessageHandlerTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public Task WhenProcessing_LinkStreetNameSqsRequest_Then_LinkStreetNameSqsLambdaRequest_IsSent()
    {
        return WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<LinkStreetNameSqsRequest, LinkStreetNameSqsLambdaRequest, LinkStreetNameRequest>();
    }

    [Fact]
    public Task WhenProcessing_UnlinkStreetNameSqsRequest_Then_UnlinkStreetNameSqsLambdaRequest_IsSent()
    {
        return WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<UnlinkStreetNameSqsRequest, UnlinkStreetNameSqsLambdaRequest, UnlinkStreetNameRequest>();
    }
    
    [Fact]
    public async Task WhenProcessingSqsRequestWithoutCorrespondingSqsLambdaRequest_ThenThrowsNotImplementedException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => mediator.Object);
        var container = containerBuilder.Build();

        var messageData = Fixture.Create<TestSqsRequest>();
        var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

        var sut = new MessageHandler(container);

        // Act
        var act = async () => await sut.HandleMessage(
            messageData,
            messageMetadata,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task WhenProcessingUnknownMessage_ThenNothingIsSent()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => mediator.Object);
        var container = containerBuilder.Build();

        var messageData = Fixture.Create<object>();
        var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

        var sut = new MessageHandler(container);

        // Act
        await sut.HandleMessage(
            messageData,
            messageMetadata,
            CancellationToken.None);

        // Assert
        mediator.VerifyNoOtherCalls();
    }

    private async Task WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<TSqsRequest, TSqsLambdaRequest, TBackOfficeRequest>()
        where TSqsRequest : SqsRequest, IHasBackOfficeRequest<TBackOfficeRequest>
        where TSqsLambdaRequest : SqsLambdaRequest, IHasBackOfficeRequest<TBackOfficeRequest>
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => mediator.Object);
        var container = containerBuilder.Build();

        var messageData = Fixture.Create<TSqsRequest>();
        var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

        var sut = new MessageHandler(container);

        // Act
        await sut.HandleMessage(
            messageData,
            messageMetadata,
            CancellationToken.None);

        // Assert
        mediator
            .Verify(x => x.Send(It.Is<TSqsLambdaRequest>(request =>
                request.TicketId == messageData.TicketId &&
                request.MessageGroupId == messageMetadata.MessageGroupId &&
                Equals(request.Request, messageData.Request) &&
                request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                request.Metadata == messageData.Metadata
            ), CancellationToken.None), Times.Once);
    }
}

internal class TestSqsRequest : SqsRequest
{
}
