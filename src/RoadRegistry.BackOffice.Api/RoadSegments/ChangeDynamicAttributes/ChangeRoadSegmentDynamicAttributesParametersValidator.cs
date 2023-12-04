namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeDynamicAttributes;

using System;
using System.Linq;
using FluentValidation;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.Extensions;

public class ChangeRoadSegmentDynamicAttributesParametersValidator : AbstractValidator<ChangeRoadSegmentDynamicAttributesParameters>
{
    public ChangeRoadSegmentDynamicAttributesParametersValidator(ChangeRoadSegmentsParametersValidatorContext validatorContext)
    {
        ArgumentNullException.ThrowIfNull(validatorContext);

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(x => x?.AantalRijstroken?.Length > 0
                       || x?.Wegbreedte?.Length > 0
                       || x?.Wegverharding?.Length > 0)
            .WithProblemCode(ProblemCode.Common.JsonInvalid);

        RuleFor(x => x.WegsegmentId)
            .Cascade(CascadeMode.Stop)
            .Must(wegsegmentId => wegsegmentId is not null && RoadSegmentId.Accepts(wegsegmentId.Value))
            .WithProblemCode(ProblemCode.Common.JsonInvalid)
            .Must(wegsegmentId => validatorContext.RoadSegmentExists(wegsegmentId!.Value))
            .WithProblemCode(ProblemCode.RoadSegment.NotFound);

        When(x => x.Wegverharding?.Length > 0, () =>
        {
            RuleFor(x => x.Wegverharding)
                .Cascade(CascadeMode.Stop)
                .Must(FirstFromPositionIsZeroOrNull)
                .WithProblemCode(ProblemCode.FromPosition.NotEqualToZero)
                .Must(PositionAttributesAreCorrectlySorted)
                .WithProblemCode(ProblemCode.ToPosition.NotEqualToNextFromPosition);

            RuleForEach(x => x.Wegverharding)
                .SetValidator(new ChangeSurfaceAttributeParametersValidator());
        });

        When(x => x.Wegbreedte?.Length > 0, () =>
        {
            RuleFor(x => x.Wegbreedte)
                .Cascade(CascadeMode.Stop)
                .Must(FirstFromPositionIsZeroOrNull)
                .WithProblemCode(ProblemCode.FromPosition.NotEqualToZero)
                .Must(PositionAttributesAreCorrectlySorted)
                .WithProblemCode(ProblemCode.ToPosition.NotEqualToNextFromPosition);

            RuleForEach(x => x.Wegbreedte)
                .SetValidator(new ChangeWidthAttributeParametersValidator());
        });

        When(x => x.AantalRijstroken?.Length > 0, () =>
        {
            RuleFor(x => x.AantalRijstroken)
                .Cascade(CascadeMode.Stop)
                .Must(FirstFromPositionIsZeroOrNull)
                .WithProblemCode(ProblemCode.FromPosition.NotEqualToZero)
                .Must(PositionAttributesAreCorrectlySorted)
                .WithProblemCode(ProblemCode.ToPosition.NotEqualToNextFromPosition);

            RuleForEach(x => x.AantalRijstroken)
                .SetValidator(new ChangeLaneAttributeParametersValidator());
        });
    }

    private bool FirstFromPositionIsZeroOrNull<T>(T[] attributes) where T : ChangePositionAttributeParameters
    {
        if (!attributes.Any())
        {
            return false;
        }

        var vanPos = attributes.First().VanPositie;
        if (vanPos is null
            || vanPos.Value.IsReasonablyEqualTo(0, (decimal)DefaultTolerances.MeasurementTolerance))
        {
            return true;
        }
        
        return false;
    }

    private bool PositionAttributesAreCorrectlySorted<T>(T[] attributes) where T : ChangePositionAttributeParameters
    {
        if (!attributes.Any())
        {
            return false;
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

                var nextVanPos = attributes[i + 1].VanPositie;
                if (nextVanPos == null)
                {
                    return false;
                }

                if (!totPos.Value.IsReasonablyEqualTo(nextVanPos.Value, (decimal)DefaultTolerances.MeasurementTolerance))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
