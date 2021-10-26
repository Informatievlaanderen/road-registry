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

    public class DownloadExtractByNisRequestBodyValidator : AbstractValidator<DownloadExtractByNisRequestBody>
    {
        private readonly EditorContext _editorContext;
        private readonly ILogger<DownloadExtractByNisRequestBodyValidator> _logger;

        public DownloadExtractByNisRequestBodyValidator(EditorContext editorContext, ILogger<DownloadExtractByNisRequestBodyValidator> logger)
        {
            _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RuleFor(c => c.Nis)
                .NotEmpty().WithMessage("'Nis' must not be empty, null or missing")
                .Must(HaveExpectedFormat).WithMessage("Invalid Nis. Expected format: '12345'")
                .MustAsync(BeKnownNis).WithMessage("'Nis' must be a known NIS code");
        }

        private bool HaveExpectedFormat(string nis)
        {
            return new Regex(@"^\d{5}$").IsMatch(nis);
        }

        private Task<bool> BeKnownNis(string nis, CancellationToken cancellationToken)
        {
            return _editorContext.MunicipalityGeometries.AnyAsync(x => x.NisCode == nis, cancellationToken);
        }
    }
}
