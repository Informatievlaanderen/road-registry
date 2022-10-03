namespace RoadRegistry.BackOffice.Handlers.Downloads;

using Abstractions.Downloads;
using FluentValidation;
using MediatR;

public sealed class DownloadProductRequestValidator : AbstractValidator<DownloadProductRequest>, IPipelineBehavior<DownloadProductRequest, DownloadProductResponse>
{
    public async Task<DownloadProductResponse> Handle(DownloadProductRequest request, RequestHandlerDelegate<DownloadProductResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
