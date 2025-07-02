namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using Abstractions.Extracts;
using FluentValidation;

public sealed class DownloadExtractByFileRequestItemValidator : AbstractValidator<DownloadExtractByFileRequestItem>
{
    public DownloadExtractByFileRequestItemValidator()
    {
        RuleFor(c => c.FileName)
            .NotEmpty().WithMessage($"'{nameof(DownloadExtractByFileRequestItem.FileName)}' must not be empty, null or missing");

        RuleFor(c => c.ContentType)
            .NotEmpty().WithMessage($"'{nameof(DownloadExtractByFileRequestItem.ContentType)}' must be able to parse");
    }
}
