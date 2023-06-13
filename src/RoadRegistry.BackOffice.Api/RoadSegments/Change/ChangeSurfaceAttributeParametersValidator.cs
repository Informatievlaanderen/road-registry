namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using Core.ProblemCodes;
using Extensions;
using FluentValidation;

public class ChangeSurfaceAttributeParametersValidator : AbstractValidator<ChangeSurfaceAttributeParameters>
{
    public ChangeSurfaceAttributeParametersValidator()
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
        
        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Type.IsRequired)
            .Must(RoadSegmentSurfaceType.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.Type.NotValid)
            ;
    }
}
