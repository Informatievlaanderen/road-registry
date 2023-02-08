namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
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
    
    [Fact]
    public async Task WhenProcessingSqsRequestWithoutCorrespondingSqsLambdaRequest_ThenThrowsNotImplementedException()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => mediator.Object);
        var container = containerBuilder.Build();

        var messageData = ObjectProvider.Create<TestSqsRequest>();
        var messageMetadata = new MessageMetadata { MessageGroupId = ObjectProvider.Create<string>() };

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

        var messageData = ObjectProvider.Create<object>();
        var messageMetadata = new MessageMetadata { MessageGroupId = ObjectProvider.Create<string>() };

        var sut = new MessageHandler(container);

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
