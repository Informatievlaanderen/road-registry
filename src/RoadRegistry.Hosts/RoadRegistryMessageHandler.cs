namespace RoadRegistry.Hosts;

using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR;

public abstract class RoadRegistryMessageHandler : IMessageHandler
{
    private readonly ILifetimeScope _container;

    protected RoadRegistryMessageHandler(ILifetimeScope container)
    {
        _container = container;
    }

    public async Task HandleMessage(object messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
    {
        messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");

        if (messageData is not SqsRequest sqsRequest)
        {
            messageMetadata.Logger?.LogInformation($"Unable to cast '{nameof(messageData)}' as {nameof(SqsRequest)}.");
            return;
        }
        
        await using var lifetimeScope = _container.BeginLifetimeScope();
        var mediator = lifetimeScope.Resolve<IMediator>();

        var sqsLambdaRequest = ConvertToLambdaRequest(sqsRequest, messageMetadata.MessageGroupId!);
        await mediator.Send(sqsLambdaRequest, cancellationToken);
    }

    protected abstract SqsLambdaRequest ConvertToLambdaRequest(SqsRequest sqsRequest, string groupId);
}
