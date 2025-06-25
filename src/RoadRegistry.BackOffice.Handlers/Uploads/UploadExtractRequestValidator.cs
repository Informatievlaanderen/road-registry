namespace RoadRegistry.BackOffice.Handlers.Uploads;

using Abstractions.Uploads;
using FluentValidation;
using MediatR;

public sealed class UploadExtractRequestValidator : AbstractValidator<UploadExtractRequest>, IPipelineBehavior<UploadExtractRequest, UploadExtractResponse>
{
    public UploadExtractRequestValidator()
    {
        RuleFor(req => req.Archive).NotNull();
    }

    public async Task<UploadExtractResponse> Handle(UploadExtractRequest request, RequestHandlerDelegate<UploadExtractResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next(cancellationToken);
        return response;
    }
}
