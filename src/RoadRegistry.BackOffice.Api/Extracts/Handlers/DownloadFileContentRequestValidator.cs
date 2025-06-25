namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed class DownloadFileContentRequestValidator : AbstractValidator<DownloadFileContentRequest>, IPipelineBehavior<DownloadFileContentRequest, DownloadFileContentResponse>
{
    public DownloadFileContentRequestValidator()
    {
        RuleFor(c => c.DownloadId)
            .NotEmpty()
            .WithMessage($"'{nameof(DownloadFileContentRequest.DownloadId)}' must not be empty, null or missing");
    }

    public async Task<DownloadFileContentResponse> Handle(DownloadFileContentRequest request, RequestHandlerDelegate<DownloadFileContentResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
