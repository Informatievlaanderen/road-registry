namespace RoadRegistry.BackOffice.Api.Downloads;

using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RoadRegistry.BackOffice.Abstractions.Downloads;

public sealed class DownloadProductRequestValidator : AbstractValidator<DownloadProductRequest>, IPipelineBehavior<DownloadProductRequest, DownloadProductResponse>
{
    public async Task<DownloadProductResponse> Handle(DownloadProductRequest request, RequestHandlerDelegate<DownloadProductResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
