namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using FluentValidation;

public sealed class ExtractDetailsRequestValidator : AbstractValidator<ExtractDetailsRequest>
{
    public ExtractDetailsRequestValidator()
    {
    }
}
