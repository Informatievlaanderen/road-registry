namespace RoadRegistry.Model
{
    using FluentValidation;
    using Messages;

    public class ChangeRoadNetworkValidator : AbstractValidator<ChangeRoadNetwork>
    {
        public ChangeRoadNetworkValidator()
        {
            RuleFor(c => c.Changes).NotNull();
            RuleForEach(c => c.Changes).NotNull().SetValidator(new RequestedChangeValidator());
        }
    }
}
