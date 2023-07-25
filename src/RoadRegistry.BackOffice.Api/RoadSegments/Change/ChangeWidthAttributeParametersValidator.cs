namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using System;
using Core;
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
            .Must((item, x) =>
                item.VanPositie is not null
                && RoadSegmentPosition.Accepts(item.VanPositie.Value)
                && item.TotPositie is not null
                && RoadSegmentPosition.Accepts(item.TotPositie.Value)
                && new RoadSegmentPosition(item.VanPositie.Value).ToDecimal()
                    .IsReasonablyLessThan(new RoadSegmentPosition(item.TotPositie.Value), (decimal)DefaultTolerances.DynamicRoadSegmentAttributePositionTolerance
                )
            )
            .WithProblemCode(ProblemCode.ToPosition.LessThanOrEqualFromPosition)
            ;

        RuleFor(x => x.Breedte)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithProblemCode(ProblemCode.Width.IsRequired)
            .Must(x => RoadSegmentWidth.CanParseUsingDutchName(x) && RoadSegmentWidth.ParseUsingDutchName(x).IsValidForEdit())
            .WithProblemCode(ProblemCode.Width.NotValid)
            ;
    }
}
