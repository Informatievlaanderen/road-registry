namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using Core.ProblemCodes;
using Extensions;
using FluentValidation;

public class ChangeWidthAttributeParametersValidator : AbstractValidator<ChangeWidthAttributeParameters>
{
    public ChangeWidthAttributeParametersValidator()
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

        RuleFor(x => x.Breedte)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Width.IsRequired)
            .Must(x => RoadSegmentWidth.Accepts(x!.Value))
            .WithProblemCode(ProblemCode.Width.NotValid)
            ;
    }
}
