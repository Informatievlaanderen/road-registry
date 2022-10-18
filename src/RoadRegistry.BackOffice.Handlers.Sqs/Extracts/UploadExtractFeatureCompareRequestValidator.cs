namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Abstractions.Extracts;
using FluentValidation;
using MediatR;

public sealed class UploadExtractFeatureCompareRequestValidator : AbstractValidator<UploadExtractFeatureCompareRequest>, IPipelineBehavior<UploadExtractFeatureCompareRequest, UploadExtractResponse>
{
    public async Task<UploadExtractResponse> Handle(UploadExtractFeatureCompareRequest request, RequestHandlerDelegate<UploadExtractResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}