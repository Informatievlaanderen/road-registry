namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Abstractions;
using Autofac;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using RoadRegistry.Tests.Framework;
using Xunit.Abstractions;

public class SqsLambdaTestsBase : BackOfficeLambdaTest
{
    protected SqsLambdaTestsBase(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory)
        : base(testOutputHelper, loggerFactory)
    {
    }

    protected async Task WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<TSqsRequest, TSqsLambdaRequest, TBackOfficeRequest>()
        where TSqsRequest : SqsRequest, IHasBackOfficeRequest<TBackOfficeRequest>
        where TSqsLambdaRequest : SqsLambdaRequest, IHasBackOfficeRequest<TBackOfficeRequest>
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var blobClient = new SqsMessagesBlobClient(Client, new());

        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => mediator.Object);
        containerBuilder.RegisterInstance(blobClient);
        var container = containerBuilder.Build();

        var messageData = ObjectProvider.Create<TSqsRequest>();
        var messageMetadata = new MessageMetadata { MessageGroupId = ObjectProvider.Create<string>() };

        var sut = new MessageHandler(container, container.Resolve<SqsMessagesBlobClient>());

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
