namespace RoadRegistry.BackOffice.Core;

using FluentValidation;
using RoadRegistry.RoadNetwork.ValueObjects;

public class RoadSegmentSideAttributesValidator : AbstractValidator<Messages.RoadSegmentSideAttributes>
{
    public RoadSegmentSideAttributesValidator()
    {
        RuleFor(c => c.StreetNameId)
            .Must(x => x is null || StreetNameLocalId.Accepts(x.Value))
            ;
    }
}
