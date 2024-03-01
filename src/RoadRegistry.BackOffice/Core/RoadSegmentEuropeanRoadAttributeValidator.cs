namespace RoadRegistry.BackOffice.Core;

using Extensions;
using FluentValidation;
using ProblemCodes;

public class RoadSegmentEuropeanRoadAttributeValidator : AbstractValidator<Messages.RoadSegmentEuropeanRoadAttribute>
{
    public RoadSegmentEuropeanRoadAttributeValidator()
    {
        RuleFor(c => c.AttributeId)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Number)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.RoadSegment.EuropeanRoad.IsRequired)
            .Must(EuropeanRoadNumber.CanParse)
            .WithProblemCode(ProblemCode.RoadSegment.EuropeanRoad.NotValid);
    }
}
