namespace RoadRegistry.BackOffice.Handlers.Uploads;

using Abstractions.Uploads;
using FluentValidation;

public sealed class UploadExtractRequestValidator : AbstractValidator<UploadExtractRequest>
{
    public UploadExtractRequestValidator()
    {
        RuleFor(req => req.DownloadId).NotEmpty();
        RuleFor(req => req.Archive).NotNull();
    }
}
