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
                .WithMessage("One or more road node identifiers are not unique.")
                .Must(OnlyHaveUniqueTemporaryRoadSegmentIdentifiers)
                .WithMessage("One or more road segment identifiers are not unique.")
                .Must(OnlyHaveUniqueEuropeanRoadAttributeIdentifiers)
                .WithMessage("One or more european road attribute identifiers are not unique.")
                .Must(OnlyHaveUniqueNationalRoadAttributeIdentifiers)
                .WithMessage("One or more national road attribute identifiers are not unique.")
                .Must(OnlyHaveUniqueNumberedRoadAttributeIdentifiers)
                .WithMessage("One or more national road attribute identifiers are not unique.")
                .Must(OnlyHaveUniqueLaneAttributeIdentifiers)
                .WithMessage("One or more lane attribute identifiers are not unique.")
                .Must(OnlyHaveUniqueWidthAttributeIdentifiers)
                .WithMessage("One or more width attribute identifiers are not unique.")
                .Must(OnlyHaveUniqueSurfaceAttributeIdentifiers)
                .WithMessage("One or more surface attribute identifiers are not unique.");
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

        private static bool OnlyHaveUniqueEuropeanRoadAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegment?.Lanes != null)
                .SelectMany(change => change.AddRoadSegment.Lanes.Where(item => item != null).Select(lane => lane.AttributeId))
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueNationalRoadAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegment?.Widths != null)
                .SelectMany(change => change.AddRoadSegment.Widths.Where(item => item != null).Select(width => width.AttributeId))
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueNumberedRoadAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegment?.Surfaces != null)
                .SelectMany(change => change.AddRoadSegment.Surfaces.Where(item => item != null).Select(surface => surface.AttributeId))
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueLaneAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegment?.Lanes != null)
                .SelectMany(change => change.AddRoadSegment.Lanes.Where(item => item != null).Select(lane => lane.AttributeId))
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueWidthAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegment?.Widths != null)
                .SelectMany(change => change.AddRoadSegment.Widths.Where(item => item != null).Select(width => width.AttributeId))
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueSurfaceAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegment?.Surfaces != null)
                .SelectMany(change => change.AddRoadSegment.Surfaces.Where(item => item != null).Select(surface => surface.AttributeId))
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }
    }
}
