namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests;

using System.Reflection;
using Abstractions;
using Autofac;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using MediatR;
using Moq;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Xunit.Abstractions;

[Collection("runsequential")]
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
        var blobClient = new SqsMessagesBlobClient(Client, new SqsJsonMessageSerializer(new FakeSqsOptions(), SqsJsonMessageAssemblies.Assemblies));

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
        var blobClient = new SqsMessagesBlobClient(Client, new SqsJsonMessageSerializer(new FakeSqsOptions(), SqsJsonMessageAssemblies.Assemblies));

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
