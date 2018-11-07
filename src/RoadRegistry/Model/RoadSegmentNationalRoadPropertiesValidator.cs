namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class RoadSegmentNationalRoadPropertiesValidator : AbstractValidator<RequestedRoadSegmentNationalRoadProperties>
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
