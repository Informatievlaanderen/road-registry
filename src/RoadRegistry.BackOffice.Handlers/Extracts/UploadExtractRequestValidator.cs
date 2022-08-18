namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FluentValidation;
using MediatR;

public sealed class UploadExtractRequestValidator : AbstractValidator<UploadExtractRequest>, IPipelineBehavior<UploadExtractRequest, UploadExtractResponse>
{
    public UploadExtractRequestValidator()
    {
    }
    public async Task<UploadExtractResponse> Handle(UploadExtractRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<UploadExtractResponse> next)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
