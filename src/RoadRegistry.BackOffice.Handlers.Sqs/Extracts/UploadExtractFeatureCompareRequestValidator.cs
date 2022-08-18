namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Abstractions.Extracts;
using FluentValidation;
using MediatR;

public sealed class UploadExtractFeatureCompareRequestValidator : AbstractValidator<UploadExtractFeatureCompareRequest>, IPipelineBehavior<UploadExtractFeatureCompareRequest, UploadExtractResponse>
{
    public UploadExtractFeatureCompareRequestValidator()
    {
    }

    public async Task<UploadExtractResponse> Handle(UploadExtractFeatureCompareRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<UploadExtractResponse> next)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
