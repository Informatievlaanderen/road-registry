namespace RoadRegistry.BackOffice.Handlers.Sqs.Uploads;

using Abstractions.Uploads;
using FluentValidation;
using MediatR;

public sealed class UploadExtractFeatureCompareRequestValidator : AbstractValidator<UploadExtractFeatureCompareRequest>, IPipelineBehavior<UploadExtractFeatureCompareRequest, UploadExtractFeatureCompareResponse>
{
    public UploadExtractFeatureCompareRequestValidator()
    {
        RuleFor(req => req.DownloadId).NotEmpty();
        RuleFor(req => req.Archive).NotNull();
    }

    public async Task<UploadExtractFeatureCompareResponse> Handle(UploadExtractFeatureCompareRequest request, RequestHandlerDelegate<UploadExtractFeatureCompareResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
