namespace RoadRegistry.BackOffice.Handlers.Downloads;

using Abstractions.Downloads;
using FluentValidation;
using MediatR;

public sealed class DownloadEditorRequestValidator : AbstractValidator<DownloadEditorRequest>, IPipelineBehavior<DownloadEditorRequest, DownloadEditorResponse>
{
    public DownloadEditorRequestValidator()
    {
    }

    public async Task<DownloadEditorResponse> Handle(DownloadEditorRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<DownloadEditorResponse> next)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
