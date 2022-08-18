namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FluentValidation;
using MediatR;

public sealed class DownloadFileContentRequestValidator : AbstractValidator<DownloadFileContentRequest>, IPipelineBehavior<DownloadFileContentRequest, DownloadFileContentResponse>
{
    public DownloadFileContentRequestValidator()
    {
        RuleFor(c => c.DownloadId)
            .NotEmpty()
            .WithMessage($"'{nameof(DownloadFileContentRequest.DownloadId)}' must not be empty, null or missing");
    }

    public async Task<DownloadFileContentResponse> Handle(DownloadFileContentRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<DownloadFileContentResponse> next)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
