namespace RoadRegistry.Hosts.Infrastructure;

using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using MediatR;

public class SqsMessageRequest : IRequest<SqsMessageResponse>
{
    public object? Data { get; set; }
    public MessageMetadata Metadata { get; init; }
}

public class SqsMessageResponse
{

}
