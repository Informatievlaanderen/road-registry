namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FluentValidation;
using Microsoft.Extensions.Logging;

public sealed class ExtractDetailsRequestValidator : AbstractValidator<ExtractDetailsRequest>
{
    private readonly ILogger<ExtractDetailsRequestValidator> _logger;

    public ExtractDetailsRequestValidator(ILogger<ExtractDetailsRequestValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
