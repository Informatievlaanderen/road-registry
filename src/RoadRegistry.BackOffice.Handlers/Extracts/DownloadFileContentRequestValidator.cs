namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FluentValidation;

public sealed class DownloadFileContentRequestValidator : AbstractValidator<DownloadFileContentRequest>
{
    public DownloadFileContentRequestValidator()
    {
        RuleFor(c => c.DownloadId)
            .NotEmpty()
            .WithMessage($"'{nameof(DownloadFileContentRequest.DownloadId)}' must not be empty, null or missing");
    }
}
