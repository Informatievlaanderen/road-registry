namespace RoadRegistry.Api.BackOffice.Handlers;

using Abstractions;
using Exceptions;
using FluentValidation;
using MediatR.Pipeline;
using RoadRegistry.BackOffice.Framework;

internal class DownloadExtractByContourRequestHandler :
    EndpointRequestHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse>,
    IRequestExceptionHandler<DownloadExtractByContourRequest, DownloadExtractByContourResponse, DownloadExtractByContourNotFoundException>
{
    public DownloadExtractByContourRequestHandler(CommandHandlerDispatcher dispatcher, IValidator<DownloadExtractByContourRequest> validator) : base(dispatcher, validator) { }

    public override Task<DownloadExtractByContourResponse> HandleAsync(DownloadExtractByContourRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Handle(DownloadExtractByContourRequest request, DownloadExtractByContourNotFoundException exception, RequestExceptionHandlerState<DownloadExtractByContourResponse> state, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
