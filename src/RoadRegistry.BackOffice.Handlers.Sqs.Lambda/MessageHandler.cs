namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Hosts.Infrastructure;
using MediatR;

public sealed class MessageHandler : IMessageHandler
{
    private readonly IMediator _mediator;

    public MessageHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
    {
        var messageRequest = new SqsMessageRequest
        {
            Data = messageData,
            Metadata = messageMetadata
        };
        await _mediator.Send(messageRequest, cancellationToken);
    }
}
