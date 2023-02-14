namespace RoadRegistry.Hosts.Infrastructure;

using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using MediatR;

public class SqsMessageRequest : IRequest
{
    public object? Data { get; set; }
    public MessageMetadata Metadata { get; init; }
}
