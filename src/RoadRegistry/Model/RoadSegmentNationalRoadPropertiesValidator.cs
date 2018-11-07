namespace RoadRegistry.Model
{
    using FluentValidation;
    using RoadRegistry.Commands;

    public class RoadSegmentNationalRoadPropertiesValidator : AbstractValidator<Shared.RoadSegmentNationalRoadProperties>
    {
        public RoadSegmentNationalRoadPropertiesValidator()
        {
            RuleFor(c => c.Ident2)
                .NotEmpty()
                .Must(NationalRoadNumber.CanParse)
                .When(c => c.Ident2 != null, ApplyConditionTo.CurrentValidator);
        }
    }
}
