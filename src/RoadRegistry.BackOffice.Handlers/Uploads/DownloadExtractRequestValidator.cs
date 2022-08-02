namespace RoadRegistry.BackOffice.Handlers.Uploads;

using Abstractions.Uploads;
using FluentValidation;

public sealed class DownloadExtractRequestValidator : AbstractValidator<DownloadExtractRequest>
{
    public DownloadExtractRequestValidator()
    {
        RuleFor(req => req.Identifier).NotEmpty();
    }
}
