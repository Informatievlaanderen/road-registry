namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using Core.ProblemCodes;
using Extensions;
using FluentValidation;

public class ChangeLaneAttributeParametersValidator : AbstractValidator<ChangeLaneAttributeParameters>
{
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
            ;

        RuleFor(x => x.Aantal)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Count.IsRequired)
            .Must(x => RoadSegmentLaneCount.Accepts(x!.Value))
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
