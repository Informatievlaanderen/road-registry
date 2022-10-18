namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FluentValidation;
using MediatR;

public sealed class UploadExtractRequestValidator : AbstractValidator<UploadExtractRequest>, IPipelineBehavior<UploadExtractRequest, UploadExtractResponse>
{
    public async Task<UploadExtractResponse> Handle(UploadExtractRequest request, RequestHandlerDelegate<UploadExtractResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}