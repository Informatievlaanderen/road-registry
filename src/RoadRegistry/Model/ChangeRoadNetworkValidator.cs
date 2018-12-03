namespace RoadRegistry.Model
{
    using System.Linq;
    using FluentValidation;
    using Messages;

    public class ChangeRoadNetworkValidator : AbstractValidator<ChangeRoadNetwork>
    {
        public ChangeRoadNetworkValidator()
        {
            RuleFor(c => c.Changes)
                .NotNull()
                .Must(OnlyHaveUniqueTemporaryRoadNodeIdentifiers)
                .Must(OnlyHaveUniqueTemporaryRoadSegmentIdentifiers);
            RuleForEach(c => c.Changes).NotNull().SetValidator(new RequestedChangeValidator());
        }

        private static bool OnlyHaveUniqueTemporaryRoadNodeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadNode != null)
                .Select(change => change.AddRoadNode.TemporaryId)
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueTemporaryRoadSegmentIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegment != null)
                .Select(change => change.AddRoadSegment.TemporaryId)
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }
    }
}
