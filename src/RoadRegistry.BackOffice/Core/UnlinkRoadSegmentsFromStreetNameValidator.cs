namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class UnlinkRoadSegmentsFromStreetNameValidator : AbstractValidator<Messages.UnlinkRoadSegmentsFromStreetName>
{
    public UnlinkRoadSegmentsFromStreetNameValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);
    }
}
