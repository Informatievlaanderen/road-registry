namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Autofac;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR;
using Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

public sealed class MessageHandler : IMessageHandler
{
    private readonly ILifetimeScope _container;

    public MessageHandler(ILifetimeScope container)
    {
        _container = container;
    }

    public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
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

    private static SqsLambdaRequest ConvertToLambdaRequest(SqsRequest sqsRequest, string groupId)
    {
        return sqsRequest switch
        {
            LinkStreetNameSqsRequest request => new LinkStreetNameSqsLambdaRequest(groupId, request),
            UnlinkStreetNameSqsRequest request => new UnlinkStreetNameSqsLambdaRequest(groupId, request),
            _ => throw new NotImplementedException(
                $"{sqsRequest.GetType().Name} has no corresponding {nameof(SqsLambdaRequest)} defined.")
        };
    }
}
