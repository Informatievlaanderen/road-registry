namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using Abstractions;
using Autofac;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using MediatR;
using Moq;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Xunit.Abstractions;

public sealed class MessageHandlerTests : RoadRegistryTestBase
{
    public MessageHandlerTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    // todo-rik why is this method not called?
    private async Task WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<TSqsRequest, TSqsLambdaRequest, TBackOfficeRequest>()
        where TSqsRequest : SqsRequest, IHasBackOfficeRequest<TBackOfficeRequest>
        where TSqsLambdaRequest : SqsLambdaRequest, IHasBackOfficeRequest<TBackOfficeRequest>
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => mediator.Object);
        var container = containerBuilder.Build();
        var blobClient = new SqsMessagesBlobClient(Client, new SqsJsonMessageSerializer(new FakeSqsOptions()));

        var messageData = ObjectProvider.Create<TSqsRequest>();
        var messageMetadata = new MessageMetadata { MessageGroupId = ObjectProvider.Create<string>() };

        var sut = new MessageHandler(container, blobClient);

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

    [Fact]
    public async Task WhenProcessingSqsRequestWithoutCorrespondingSqsLambdaRequest_ThenThrowsNotImplementedException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => mediator.Object);
        var container = containerBuilder.Build();
        var blobClient = new SqsMessagesBlobClient(Client, new SqsJsonMessageSerializer(new FakeSqsOptions()));

        var messageData = ObjectProvider.Create<TestSqsRequest>();
        var messageMetadata = new MessageMetadata { MessageGroupId = ObjectProvider.Create<string>() };

        var sut = new MessageHandler(container, blobClient);

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
        var blobClient = new SqsMessagesBlobClient(Client, new SqsJsonMessageSerializer(new FakeSqsOptions()));

        var messageData = ObjectProvider.Create<object>();
        var messageMetadata = new MessageMetadata { MessageGroupId = ObjectProvider.Create<string>() };

        var sut = new MessageHandler(container, blobClient);

        // Act
        await sut.HandleMessage(
            messageData,
            messageMetadata,
            CancellationToken.None);

        // Assert
        mediator.VerifyNoOtherCalls();
    }
}

internal class TestSqsRequest : SqsRequest
{
}
