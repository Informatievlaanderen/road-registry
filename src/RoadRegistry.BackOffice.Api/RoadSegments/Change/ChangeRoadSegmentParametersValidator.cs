namespace RoadRegistry.BackOffice.Api.RoadSegments.Change;

using System;
using System.Linq;
using Core;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;

public class ChangeRoadSegmentParametersValidator : AbstractValidator<ChangeRoadSegmentParameters>
{
    public ChangeRoadSegmentParametersValidator(ChangeRoadSegmentsParametersValidatorContext validatorContext)
    {
        ArgumentNullException.ThrowIfNull(validatorContext);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x?.AantalRijstroken is not null
                       || x?.Wegbreedte is not null
                       || x?.Wegverharding is not null)
            .WithProblemCode(ProblemCode.Common.JsonInvalid);

        RuleFor(x => x.WegsegmentId)
            .Cascade(CascadeMode.Stop)
            .Must(wegsegmentId => wegsegmentId is not null && RoadSegmentId.Accepts(wegsegmentId.Value))
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .Must(wegsegmentId => validatorContext.RoadSegmentExists(wegsegmentId!.Value))
            .WithProblemCode(ProblemCode.RoadSegment.NotFound);

        When(x => x.Wegverharding is not null, () =>
        {
            RuleFor(x => x.Wegverharding)
                .Cascade(CascadeMode.Stop)
                .Must(PositionAttributesMustBeSorted)
                .WithProblemCode(ProblemCode.ToPosition.NotEqualToNextFromPosition);

            RuleForEach(x => x.Wegverharding)
                .SetValidator(new ChangeSurfaceAttributeParametersValidator());
        });

        When(x => x.Wegbreedte is not null, () =>
        {
            RuleFor(x => x.Wegbreedte)
                .Cascade(CascadeMode.Stop)
                .Must(PositionAttributesMustBeSorted)
                .WithProblemCode(ProblemCode.ToPosition.NotEqualToNextFromPosition);

            RuleForEach(x => x.Wegbreedte)
                .SetValidator(new ChangeWidthAttributeParametersValidator());
        });

        When(x => x.AantalRijstroken is not null, () =>
        {
            RuleFor(x => x.AantalRijstroken)
                .Cascade(CascadeMode.Stop)
                .Must(PositionAttributesMustBeSorted)
                .WithProblemCode(ProblemCode.ToPosition.NotEqualToNextFromPosition);

            RuleForEach(x => x.AantalRijstroken)
                .SetValidator(new ChangeLaneAttributeParametersValidator());
        });
    }

    private bool PositionAttributesMustBeSorted<T>(T[] attributes) where T : ChangePositionAttributeParameters
    {
        if (!attributes.Any())
        {
            return true;
        }

        if (attributes.Length > 1)
        {
            for (var i = 0; i < attributes.Length - 1; i++)
            {
                var totPos = attributes[i].TotPositie;
                if (totPos == null)
                {
                    return false;
                }

                var vanPos = attributes[i + 1].VanPositie;
                if (vanPos == null)
                {
                    return false;
                }

                if (!totPos.Value.EqualsWithTolerance(vanPos.Value, (decimal)DefaultTolerances.DynamicRoadSegmentAttributePositionTolerance))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
