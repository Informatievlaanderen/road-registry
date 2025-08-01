namespace RoadRegistry.BackOffice.Handlers.Extracts.V2;

using Abstractions.Extracts;
using FluentValidation;

public sealed class ExtractDetailsRequestValidator : AbstractValidator<ExtractDetailsRequest>
{
    public ExtractDetailsRequestValidator()
    {
    }
}
