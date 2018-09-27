namespace RoadRegistry.Model
{
    using Commands;
    using FluentValidation;

    public class ChangeRoadNetworkValidator : AbstractValidator<ChangeRoadNetwork>
    {
        public ChangeRoadNetworkValidator()
        {
            RuleFor(c => c.Changes).NotNull();
            RuleForEach(c => c.Changes).NotNull().SetValidator(new RoadNetworkChangeValidator());
        }
    }
}