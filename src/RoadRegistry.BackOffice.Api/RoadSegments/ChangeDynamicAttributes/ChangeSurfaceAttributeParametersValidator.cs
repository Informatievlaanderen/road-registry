namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using System;
using FluentValidation;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.Extensions;

public class ChangeSurfaceAttributeParametersValidator : AbstractValidator<ChangeSurfaceAttributeParameters>
{
    private readonly VerificationContextTolerances _tolerances = VerificationContextTolerances.Default;

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
        
        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Type.IsRequired)
            .Must(RoadSegmentSurfaceType.CanParseUsingDutchName)
            .WithProblemCode(ProblemCode.Type.NotValid)
            ;
    }
}
