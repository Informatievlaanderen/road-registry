namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FluentValidation;
using MediatR;

public sealed class UploadStatusRequestValidator : AbstractValidator<UploadStatusRequest>, IPipelineBehavior<UploadStatusRequest, UploadStatusResponse>
{
    public async Task<UploadStatusResponse> Handle(UploadStatusRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<UploadStatusResponse> next)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
