namespace RoadRegistry.BackOffice.Handlers.Sqs.Uploads;

using Abstractions.Uploads;
using FluentValidation;

public sealed class UploadExtractFeatureCompareRequestValidator : AbstractValidator<UploadExtractFeatureCompareRequest>
{
    public UploadExtractFeatureCompareRequestValidator()
    {
        RuleFor(req => req.DownloadId).NotEmpty();
        RuleFor(req => req.Archive).NotNull();
    }
}
