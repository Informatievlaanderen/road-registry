namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using Abstractions;
using Autofac;
using AutoFixture;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Editor.Projections;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using RoadRegistry.BackOffice.Extracts.Dbase.Organizations;
using RoadRegistry.Tests.Framework;
using Xunit.Abstractions;

public class SqsLambdaTestsBase : BackOfficeLambdaTest
{
    protected SqsLambdaTestsBase(ITestOutputHelper testOutputHelper, ILoggerFactory loggerFactory)
        : base(testOutputHelper, loggerFactory)
    {
        EditorContext.Organizations.Add(
            new OrganizationRecord
            {
                Code = "AGIV",
                SortableCode = "AGIV",
                DbaseSchemaVersion = BackOffice.Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord.DbaseSchemaVersion,
                DbaseRecord = new BackOffice.Extracts.Dbase.Organizations.V1.OrganizationDbaseRecord
                {
                    ORG = { Value = "AGIV" },
                    LBLORG = { Value = "Agentschap voor Geografische Informatie Vlaanderen" }
                }.ToBytes(RecyclableMemoryStreamManager, FileEncoding)
            });
        EditorContext.SaveChanges();
    }

    protected async Task WhenProcessing_SqsRequest_Then_SqsLambdaRequest_IsSent<TSqsRequest, TSqsLambdaRequest, TBackOfficeRequest>()
        where TSqsRequest : SqsRequest, IHasBackOfficeRequest<TBackOfficeRequest>
        where TSqsLambdaRequest : SqsLambdaRequest, IHasBackOfficeRequest<TBackOfficeRequest>
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var blobClient = new SqsMessagesBlobClient(Client, new SqsJsonMessageSerializer(new SqsOptions()));

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
            .Verify(x => x.Send<SqsLambdaRequest>(It.Is<TSqsLambdaRequest>(request =>
                request.TicketId == messageData.TicketId &&
                request.MessageGroupId == messageMetadata.MessageGroupId &&
                Equals(request.Request, messageData.Request) &&
                request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                request.Metadata == messageData.Metadata
            ), CancellationToken.None), Times.Once);
    }
}
