namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed class UploadStatusRequestValidator : AbstractValidator<UploadStatusRequest>, IPipelineBehavior<UploadStatusRequest, UploadStatusResponse>
{
    public async Task<UploadStatusResponse> Handle(UploadStatusRequest request, RequestHandlerDelegate<UploadStatusResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }
}
