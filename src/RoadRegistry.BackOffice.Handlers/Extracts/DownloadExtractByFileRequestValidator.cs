namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FluentValidation;
using MediatR;

public sealed class DownloadExtractByFileRequestValidator : AbstractValidator<DownloadExtractByFileRequest>, IPipelineBehavior<DownloadExtractByFileRequest, DownloadExtractByFileResponse>
{
    public DownloadExtractByFileRequestValidator()
    {
        RuleFor(c => c.ShpFile)
            .SetValidator(new DownloadExtractByFileRequestItemValidator());
        RuleFor(c => c.ShxFile)
            .SetValidator(new DownloadExtractByFileRequestItemValidator());
        RuleFor(c => c.PrjFile)
            .SetValidator(new DownloadExtractByFileRequestItemValidator());

        RuleFor(c => c.Buffer)
            .InclusiveBetween(0, 100).WithMessage("'Buffer' must be a value between 0 and 100");

        RuleFor(c => c.Description)
            .NotNull().WithMessage("'Description' must not be null or missing")
            .MaximumLength(ExtractDescription.MaxLength).WithMessage($"'Description' must not be longer than {ExtractDescription.MaxLength} characters");
    }

    public async Task<DownloadExtractByFileResponse> Handle(DownloadExtractByFileRequest request, RequestHandlerDelegate<DownloadExtractByFileResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
