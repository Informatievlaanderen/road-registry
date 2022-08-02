namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System.Text.RegularExpressions;
using Abstractions.Extracts;
using Editor.Schema;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public sealed class DownloadExtractByNisCodeRequestValidator : AbstractValidator<DownloadExtractByNisCodeRequest>
{
    private readonly EditorContext _editorContext;

    public DownloadExtractByNisCodeRequestValidator(EditorContext editorContext)
    {
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));

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

    private static bool BeNisCodeWithExpectedFormat(string nisCode)
    {
        return new Regex(@"^\d{5}$").IsMatch(nisCode);
    }

    private Task<bool> BeKnownNisCode(string nisCode, CancellationToken cancellationToken)
    {
        return _editorContext.MunicipalityGeometries.AnyAsync(x => x.NisCode == nisCode, cancellationToken);
    }
}
