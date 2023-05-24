namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Exceptions;
using Framework;
using Microsoft.Extensions.Logging;


public abstract class ExtractRequestHandler<TRequest, TResponse> : EndpointRequestHandler<TRequest, TResponse>
    where TRequest : EndpointRequest<TResponse>
    where TResponse : EndpointResponse
{
    protected readonly EditorContext _context;

    protected ExtractRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger logger) : base(dispatcher, logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    protected async Task DispatchCommandAfterConditionalContextUpsertAsync(ExtractRequestRecord record, object message, CancellationToken cancellationToken)
    {
        var command = new Command(message);
        await Dispatcher(command, cancellationToken);
    }

    public override async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");

        return await HandleRequestAsync(request,  downloadId, randomExternalRequestId, cancellationToken);
    }

    public abstract Task<TResponse> HandleRequestAsync(TRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken);
}

public class ExtractDetailsRequestHandler : ExtractRequestHandler<ExtractDetailsRequest, ExtractDetailsResponse>
{

    public ExtractDetailsRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(context, dispatcher, logger)
    {
    }

    public override async Task<ExtractDetailsResponse> HandleAsync(ExtractDetailsRequest request, CancellationToken cancellationToken)
    {
        var record = await _context.ExtractRequests.FindAsync(new object[] { request.DownloadId.ToString() }, cancellationToken)
            ?? throw new ExtractRequestNotFoundException(request.DownloadId);

        return new ExtractDetailsResponse
        {
            DownloadId = new DownloadId(record.DownloadId),
            Description = record.Description,
            ExtractRequestId = ExtractRequestId.FromString(record.RequestId),
            ExternalRequestId = record.ExternalRequestId,
            Available = record.Available,
            AvailableOn = new DateTime(record.AvailableOn),
            RequestId = record.RequestId,
            RequestOn = new DateTime(record.RequestedOn),
            UploadExpected = record.UploadExpected,
        };
    }
}
