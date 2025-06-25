namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Editor.Schema;
using FluentValidation;
using MediatR;

public sealed class UploadExtractRequestValidator : AbstractValidator<UploadExtractRequest>, IPipelineBehavior<UploadExtractRequest, UploadExtractResponse>
{
    private readonly EditorContext _editorContext;
    private readonly IExtractUploadFailedEmailClient _emailClient;

    public UploadExtractRequestValidator(EditorContext editorContext, IExtractUploadFailedEmailClient emailClient)
    {
        _editorContext = editorContext;
        _emailClient = emailClient;
    }

    public async Task<UploadExtractResponse> Handle(UploadExtractRequest request, RequestHandlerDelegate<UploadExtractResponse> next, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            var response = await next(cancellationToken);
            return response;
        }

        var ex = new ValidationException(validationResult.Errors);
        var extractRequest = await _editorContext.ExtractRequests.FindAsync([DownloadId.Parse(request.DownloadId).ToGuid()], cancellationToken);

        await _emailClient.SendAsync(extractRequest?.Description, ex, cancellationToken);
        throw ex;
    }
}
