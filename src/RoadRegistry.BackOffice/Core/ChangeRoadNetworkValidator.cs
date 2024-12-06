namespace RoadRegistry.BackOffice.Core;

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
            .Must(OnlyHaveUniqueTemporaryRoadNodeIdentifiers)
            .WithMessage("One or more temporary road node identifiers are not unique.")
            .Must(OnlyHaveUniquePermanentRoadNodeIdentifiers)
            .WithMessage("One or more permanent road node identifiers are not unique.")
            .Must(OnlyHaveUniqueTemporaryRoadSegmentIdentifiers)
            .WithMessage("One or more temporary road segment identifiers are not unique.")
            .Must(OnlyHaveUniquePermanentRoadSegmentIdentifiers)
            .WithMessage("One or more permanent road segment identifiers are not unique.")
            .Must(OnlyHaveUniqueTemporaryEuropeanRoadAttributeIdentifiers)
            .WithMessage("One or more temporary european road attribute identifiers are not unique.")
            .Must(OnlyHaveUniquePermanentEuropeanRoadAttributeIdentifiers)
            .WithMessage("One or more permanent european road attribute identifiers are not unique.")
            .Must(OnlyHaveUniqueTemporaryNationalRoadAttributeIdentifiers)
            .WithMessage("One or more temporary national road attribute identifiers are not unique.")
            .Must(OnlyHaveUniqueRemovedNationalRoadAttributeIdentifiers)
            .WithMessage("One or more removed national road attribute identifiers are not unique.")
            .Must(OnlyHaveUniqueTemporaryNumberedRoadAttributeIdentifiers)
            .WithMessage("One or more temporary numbered road attribute identifiers are not unique.")
            .Must(OnlyHaveUniqueRemovedNumberedRoadAttributeIdentifiers)
            .WithMessage("One or more removed numbered road attribute identifiers are not unique.")
            .Must(OnlyHaveUniqueLaneAttributeIdentifiers)
            .WithMessage("One or more lane attribute identifiers are not unique.")
            .Must(OnlyHaveUniqueWidthAttributeIdentifiers)
            .WithMessage("One or more width attribute identifiers are not unique.")
            .Must(OnlyHaveUniqueSurfaceAttributeIdentifiers)
            .WithMessage("One or more surface attribute identifiers are not unique.")
            .Must(OnlyHaveUniqueTemporaryGradeSeparatedIdentifiers)
            .WithMessage("One or more temporary grade separated junction identifiers are not unique.")
            .Must(OnlyHaveUniquePermanentGradeSeparatedIdentifiers)
            .WithMessage("One or more permanent grade separated junction identifiers are not unique.")
            ;
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
                    change.ModifyRoadNode,
                    change.RemoveRoadNode,
                    change.AddRoadSegment,
                    change.ModifyRoadSegment,
                    change.ModifyRoadSegmentAttributes,
                    change.ModifyRoadSegmentGeometry,
                    change.RemoveRoadSegment,
                    change.RemoveOutlinedRoadSegment,
                    change.RemoveOutlinedRoadSegmentFromRoadNetwork,
                    change.AddRoadSegmentToEuropeanRoad,
                    change.RemoveRoadSegmentFromEuropeanRoad,
                    change.AddRoadSegmentToNationalRoad,
                    change.RemoveRoadSegmentFromNationalRoad,
                    change.AddRoadSegmentToNumberedRoad,
                    change.RemoveRoadSegmentFromNumberedRoad,
                    change.AddGradeSeparatedJunction,
                    change.ModifyGradeSeparatedJunction,
                    change.RemoveGradeSeparatedJunction
                }
                .Count(changeEvent => !ReferenceEquals(changeEvent, null)) == 1;
    }

    private static bool OnlyHaveUniqueLaneAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.AddRoadSegment?.Lanes != null)
            .SelectMany(change => change.AddRoadSegment.Lanes.Where(item => item != null).Select(lane => lane.AttributeId))
            .Union(changes
                .Where(change => change?.ModifyRoadSegment?.Lanes != null)
                .SelectMany(change => change.ModifyRoadSegment.Lanes.Where(item => item != null).Select(lane => lane.AttributeId))
            )
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniquePermanentEuropeanRoadAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.RemoveRoadSegmentFromEuropeanRoad != null)
            .Select(change => change.RemoveRoadSegmentFromEuropeanRoad.AttributeId)
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniquePermanentGradeSeparatedIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.ModifyGradeSeparatedJunction != null)
            .Select(change => change.ModifyGradeSeparatedJunction.Id)
            .Union(
                changes
                    .Where(change => change?.RemoveGradeSeparatedJunction != null)
                    .Select(change => change.RemoveGradeSeparatedJunction.Id)
            )
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniqueRemovedNationalRoadAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.RemoveRoadSegmentFromNationalRoad != null)
            .Select(change => change.RemoveRoadSegmentFromNationalRoad.AttributeId)
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniqueRemovedNumberedRoadAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.RemoveRoadSegmentFromNumberedRoad != null)
            .Select(change => change.RemoveRoadSegmentFromNumberedRoad.AttributeId)
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniquePermanentRoadNodeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.ModifyRoadNode != null)
            .Select(change => change.ModifyRoadNode.Id)
            .Union(
                changes
                    .Where(change => change?.RemoveRoadNode != null)
                    .Select(change => change.RemoveRoadNode.Id)
            )
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniquePermanentRoadSegmentIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.ModifyRoadSegment != null)
            .Select(change => change.ModifyRoadSegment.Id)
            .Union(
                changes
                    .Where(change => change?.RemoveRoadSegment != null)
                    .Select(change => change.RemoveRoadSegment.Id))
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniqueSurfaceAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.AddRoadSegment?.Surfaces != null)
            .SelectMany(change => change.AddRoadSegment.Surfaces.Where(item => item != null).Select(surface => surface.AttributeId))
            .Union(
                changes
                    .Where(change => change?.ModifyRoadSegment?.Surfaces != null)
                    .SelectMany(change => change.ModifyRoadSegment.Surfaces.Where(item => item != null).Select(surface => surface.AttributeId))
            )
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniqueTemporaryEuropeanRoadAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.AddRoadSegmentToEuropeanRoad != null)
            .Select(change => change.AddRoadSegmentToEuropeanRoad.TemporaryAttributeId)
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniqueTemporaryGradeSeparatedIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.AddGradeSeparatedJunction != null)
            .Select(change => change.AddGradeSeparatedJunction.TemporaryId)
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniqueTemporaryNationalRoadAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.AddRoadSegmentToNationalRoad != null)
            .Select(change => change.AddRoadSegmentToNationalRoad.TemporaryAttributeId)
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }

    private static bool OnlyHaveUniqueTemporaryNumberedRoadAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.AddRoadSegmentToNumberedRoad != null)
            .Select(change => change.AddRoadSegmentToNumberedRoad.TemporaryAttributeId)
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
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

    private static bool OnlyHaveUniqueWidthAttributeIdentifiers(RequestedChange[] changes)
    {
        if (changes == null) return true;

        var identifiers = changes
            .Where(change => change?.AddRoadSegment?.Widths != null)
            .SelectMany(change => change.AddRoadSegment.Widths.Where(item => item != null).Select(width => width.AttributeId))
            .Union(changes
                .Where(change => change?.ModifyRoadSegment?.Widths != null)
                .SelectMany(change => change.ModifyRoadSegment.Widths.Where(item => item != null).Select(width => width.AttributeId))
            )
            .ToArray();
        return identifiers.Length == identifiers.Distinct().Count();
    }
}
