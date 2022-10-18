namespace RoadRegistry.BackOffice.Handlers.Uploads;

using Abstractions.Uploads;
using FluentValidation;
using MediatR;

public sealed class DownloadExtractRequestValidator : AbstractValidator<DownloadExtractRequest>, IPipelineBehavior<DownloadExtractRequest, DownloadExtractResponse>
{
    public DownloadExtractRequestValidator()
    {
        RuleFor(req => req.Identifier).NotEmpty();
    }

    public async Task<DownloadExtractResponse> Handle(DownloadExtractRequest request, RequestHandlerDelegate<DownloadExtractResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}