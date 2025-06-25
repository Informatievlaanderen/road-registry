namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using FluentValidation;
using RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed class ExtractDetailsRequestValidator : AbstractValidator<ExtractDetailsRequest>
{
    public ExtractDetailsRequestValidator()
    {
    }
}
