namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;
    using FluentValidation;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class DownloadExtractByNisCodeRequestBodyValidator : AbstractValidator<DownloadExtractByNisCodeRequestBody>
    {
        private readonly EditorContext _editorContext;
        private readonly ILogger<DownloadExtractByNisCodeRequestBodyValidator> _logger;

        public DownloadExtractByNisCodeRequestBodyValidator(EditorContext editorContext, ILogger<DownloadExtractByNisCodeRequestBodyValidator> logger)
        {
            _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RuleFor(c => c.NisCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("'NisCode' must not be empty, null or missing")
                .Must(HaveExpectedFormat).WithMessage("Invalid NIS-code. Expected format: '12345'")
                .MustAsync(BeKnownNisCode).WithMessage("'NisCode' must be a known NIS-code");
        }

        private bool HaveExpectedFormat(string nisCode)
        {
            return new Regex(@"^\d{5}$").IsMatch(nisCode);
        }

        private Task<bool> BeKnownNisCode(string nisCode, CancellationToken cancellationToken)
        {
            return _editorContext.MunicipalityGeometries.AnyAsync(x => x.NisCode == nisCode, cancellationToken);
        }
    }
}
