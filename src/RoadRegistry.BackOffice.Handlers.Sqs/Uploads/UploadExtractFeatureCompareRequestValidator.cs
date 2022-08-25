namespace RoadRegistry.BackOffice.Handlers.Sqs.Uploads;

using Abstractions.Uploads;
using FluentValidation;
using MediatR;

public sealed class UploadExtractFeatureCompareRequestValidator : AbstractValidator<UploadExtractFeatureCompareRequest>, IPipelineBehavior<UploadExtractFeatureCompareRequest, UploadExtractResponse>
{
    public UploadExtractFeatureCompareRequestValidator()
    {
        RuleFor(req => req.DownloadId).NotEmpty();
        RuleFor(req => req.Archive).NotNull();
    }

    public async Task<UploadExtractResponse> Handle(UploadExtractFeatureCompareRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<UploadExtractResponse> next)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
