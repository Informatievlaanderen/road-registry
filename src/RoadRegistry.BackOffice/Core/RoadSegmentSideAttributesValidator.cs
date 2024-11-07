namespace RoadRegistry.BackOffice.Core;

using FluentValidation;

public class RoadSegmentSideAttributesValidator : AbstractValidator<Messages.RoadSegmentSideAttributes>
{
    public RoadSegmentSideAttributesValidator()
    {
        RuleFor(c => c.StreetNameId)
            .Must(x => x is null || StreetNameLocalId.Accepts(x.Value))
            ;
    }
}
