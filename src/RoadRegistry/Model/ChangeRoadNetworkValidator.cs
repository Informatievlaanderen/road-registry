namespace RoadRegistry.Model
{
    using Commands;
    using FluentValidation;

    public class ChangeRoadNetworkValidator : AbstractValidator<ChangeRoadNetwork>
    {
        public ChangeRoadNetworkValidator()
        {
            RuleFor(c => c.Changeset).NotNull();
            RuleForEach(c => c.Changeset).NotNull().SetValidator(new RoadNetworkChangeValidator());
        }
    }
}