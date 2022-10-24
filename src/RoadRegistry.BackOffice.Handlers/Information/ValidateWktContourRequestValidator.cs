namespace RoadRegistry.BackOffice.Handlers.Information;

using Abstractions.Information;
using FluentValidation;

public sealed class ValidateWktContourRequestValidator : AbstractValidator<ValidateWktContourRequest>
{
    public ValidateWktContourRequestValidator()
    {
        // Validation MUST be done inside the handler instead of through pipeline behaviour
    }
}
