namespace RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeDynamicAttributes;

using FluentValidation;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.ValueObjects.ProblemCodes;

public class ChangeLaneAttributeParametersValidator : AbstractValidator<ChangeLaneAttributeParameters>
{
    private readonly VerificationContextTolerances _tolerances = VerificationContextTolerances.Default;

    public ChangeLaneAttributeParametersValidator()
    {
        RuleFor(x => x.VanPositie)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.FromPosition.IsRequired)
            .Must(x => RoadSegmentPosition.Accepts(x!.Value))
            .WithProblemCode(ProblemCode.FromPosition.NotValid)
            ;

        RuleFor(x => x.TotPositie)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.ToPosition.IsRequired)
            .Must(x => RoadSegmentPosition.Accepts(x!.Value))
            .WithProblemCode(ProblemCode.ToPosition.NotValid)
            .Must((item, x) =>
                item.VanPositie is not null
                && RoadSegmentPosition.Accepts(item.VanPositie.Value)
                && item.TotPositie is not null
                && RoadSegmentPosition.Accepts(item.TotPositie.Value)
                && new RoadSegmentPosition(item.VanPositie.Value).ToDecimal()
                    .IsReasonablyLessThan(new RoadSegmentPosition(item.TotPositie.Value), _tolerances
                )
            )
            .WithProblemCode(ProblemCode.ToPosition.LessThanOrEqualFromPosition)
            ;

        RuleFor(x => x.Aantal)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Count.IsRequired)
            .Must(x => RoadSegmentLaneCount.CanParseUsingDutchName(x) && RoadSegmentLaneCount.ParseUsingDutchName(x).IsValidForEdit())
            .WithProblemCode(ProblemCode.Count.NotValid)
            ;

        RuleFor(x => x.Richting)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Direction.IsRequired)
            .Must(RoadSegmentLaneDirection.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.Direction.NotValid)
            ;
    }
}
