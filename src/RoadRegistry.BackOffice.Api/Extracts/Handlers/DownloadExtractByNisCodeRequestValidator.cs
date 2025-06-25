namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using FluentValidation;
using MediatR;
using Sync.MunicipalityRegistry;

public sealed class DownloadExtractByNisCodeRequestValidator : AbstractValidator<DownloadExtractByNisCodeRequest>, IPipelineBehavior<DownloadExtractByNisCodeRequest, DownloadExtractByNisCodeResponse>
{
    private readonly MunicipalityEventConsumerContext _municipalityContext;

    public DownloadExtractByNisCodeRequestValidator(MunicipalityEventConsumerContext editorContext)
    {
        _municipalityContext = editorContext.ThrowIfNull();

        RuleFor(c => c.NisCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("'NisCode' must not be empty, null or missing")
            .Must(BeNisCodeWithExpectedFormat).WithMessage("Invalid NIS-code. Expected format: '12345'")
            .MustAsync(BeKnownNisCode).WithMessage("'NisCode' must be a known NIS-code");

        RuleFor(c => c.Buffer)
            .InclusiveBetween(0, 100).WithMessage("'Buffer' must be a value between 0 and 100");

        RuleFor(c => c.Description)
            .NotNull().WithMessage("'Description' must not be null or missing")
            .MaximumLength(ExtractDescription.MaxLength).WithMessage($"'Description' must not be longer than {ExtractDescription.MaxLength} characters");
    }

    public async Task<DownloadExtractByNisCodeResponse> Handle(DownloadExtractByNisCodeRequest request, RequestHandlerDelegate<DownloadExtractByNisCodeResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }

    private Task<bool> BeKnownNisCode(string nisCode, CancellationToken cancellationToken)
    {
        return _municipalityContext.CurrentMunicipalityExistsByNisCode(nisCode, cancellationToken);
    }

    private static bool BeNisCodeWithExpectedFormat(string nisCode)
    {
        return new Regex(@"^\d{5}$").IsMatch(nisCode);
    }
}
