namespace RoadRegistry.BackOffice.Core
{
    using System.Linq;
    using FluentValidation;
    using Messages;

    public class ChangeRoadNetworkValidator : AbstractValidator<ChangeRoadNetwork>
    {
        public ChangeRoadNetworkValidator()
        {
            RuleFor(c => c.RequestId)
                .NotNull()
                .NotEmpty()
                .Length(ChangeRequestId.ExactStringLength);

            RuleFor(c => c.Reason)
                .NotNull()
                .NotEmpty()
                .MaximumLength(Reason.MaxLength);

            RuleFor(c => c.Operator)
                .NotNull()
                .NotEmpty()
                .MaximumLength(OperatorName.MaxLength);

            RuleFor(c => c.OrganizationId)
                .NotNull()
                .NotEmpty()
                .MaximumLength(OrganizationId.MaxLength);

            RuleFor(c => c.Changes)
                .NotNull()
                .Must(OnlyHaveUniqueRoadNodeIdentifiers)
                .WithMessage("One or more road node identifiers are not unique.")
                .Must(OnlyHaveUniqueRoadSegmentIdentifiers)
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
                .WithMessage("One or more surface attribute identifiers are not unique.")
                .Must(OnlyHaveUniqueGradeSeparatedIdentifiers)
                .WithMessage("One or more grade separated junction identifiers are not unique.");
            RuleForEach(c => c.Changes)
                .NotNull()
                .Must(HaveExactlyOneNonNullChange)
                .WithMessage("Exactly one change must be not null.")
                .SetValidator(new RequestedChangeValidator());
        }

        private static bool HaveExactlyOneNonNullChange(RequestedChange change)
        {
            if (change == null) return true;

            return
                new object[]
                    {
                        change.AddRoadNode,
                        change.AddRoadSegment,
                        change.AddRoadSegmentToEuropeanRoad,
                        change.AddRoadSegmentToNationalRoad,
                        change.AddRoadSegmentToNumberedRoad,
                        change.AddGradeSeparatedJunction
                    }
                    .Count(_ => !ReferenceEquals(_, null)) == 1;
        }

        private static bool OnlyHaveUniqueRoadNodeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadNode != null)
                .Select(change => change.AddRoadNode.TemporaryId)
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueRoadSegmentIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegment != null)
                .Select(change => change.AddRoadSegment.TemporaryId)
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueGradeSeparatedIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddGradeSeparatedJunction != null)
                .Select(change => change.AddGradeSeparatedJunction.TemporaryId)
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueEuropeanRoadAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegmentToEuropeanRoad != null)
                .Select(change => change.AddRoadSegmentToEuropeanRoad.TemporaryAttributeId)
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueNationalRoadAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegmentToNationalRoad != null)
                .Select(change => change.AddRoadSegmentToNationalRoad.TemporaryAttributeId)
                .ToArray();
            return identifiers.Length == identifiers.Distinct().Count();
        }

        private static bool OnlyHaveUniqueNumberedRoadAttributeIdentifiers(RequestedChange[] changes)
        {
            if (changes == null) return true;

            var identifiers = changes
                .Where(change => change?.AddRoadSegmentToNumberedRoad != null)
                .Select(change => change.AddRoadSegmentToNumberedRoad.TemporaryAttributeId)
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
